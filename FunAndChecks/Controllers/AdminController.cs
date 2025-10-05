using System.Security.Claims;
using FunAndChecks.Data;
using FunAndChecks.DTO;
using FunAndChecks.Hub;
using FunAndChecks.Models;
using FunAndChecks.Models.Enums;
using FunAndChecks.Models.JoinTables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FunAndChecks.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController(
    ApplicationDbContext context,
    IHubContext<QueueHub> hubContext,
    IHubContext<ResultsHub> resultsHubContext)
    : ControllerBase
{
    private Guid GetCurrentAdminId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                                                   throw new InvalidOperationException("Admin not found"));

    [HttpPost("create/subject")]
    [ProducesResponseType(typeof(SubjectDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<SubjectDto>> CreateSubject(CreateSubjectDto dto)
    {
        var subject = new Subject { Name = dto.Name };
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();
    
        var responseDto = new SubjectDto(subject.Id, subject.Name);

        return CreatedAtAction(nameof(CreateSubject), new { id = subject.Id }, responseDto);
    }
    
    [HttpPost("create/group")]
    [ProducesResponseType(typeof(GroupDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<GroupDto>> CreateGroup(CreateGroupDto dto)
    {
        var group = new Group { Name = dto.Name, GroupNumber = dto.GroupNumber, StartYear = dto.StartYear };
        context.Groups.Add(group);
        await context.SaveChangesAsync();
    
        var responseDto = new GroupDto(group.Id, group.Name, group.StartYear, group.GroupNumber);

        return CreatedAtAction(nameof(CreateGroup), new { id = group.Id }, responseDto);
    }

    [HttpPost("create/task")]
    [ProducesResponseType(typeof(CreateTaskDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateTaskDto>> CreateTask(CreateTaskDto dto)
    {
        var subjectExists = await context.Subjects.AnyAsync(s => s.Id == dto.SubjectId);
        if (!subjectExists) return NotFound("Subject not found.");

        var task = new CourseTask
        {
            Name = dto.Name,
            Description = dto.Description,
            MaxPoints = dto.MaxPoints,
            SubjectId = dto.SubjectId
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(CreateTask), new { id = task.Id }, dto);
    }

    /// <summary>
    /// Позволяет отправить попытку (успешную или не очень) сдать задание
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("create/submission")]
    public async Task<IActionResult> CreateSubmission(CreateSubmissionDto dto)
    {
        var adminId = GetCurrentAdminId();
        var submission = new UserTaskSubmission
        {
            UserId = dto.UserId,
            TaskId = dto.TaskId,
            Status = dto.Status,
            Comment = dto.Comment,
            AdminId = adminId,
            SubmissionDate = DateTime.UtcNow
        };
        context.UserTaskSubmissions.Add(submission);
        await context.SaveChangesAsync();

        var task = await context.Tasks.FindAsync(dto.TaskId);
        if (task != null)
        {
            string groupName = $"results-subject-{task.SubjectId}";
            var updateDto = new ResultUpdateDto(dto.UserId, dto.TaskId, dto.Status.ToString());

            await resultsHubContext.Clients.Group(groupName).SendAsync("ResultUpdated", updateDto);
        }

        return Ok();
    }

    [HttpPost("create/queue-event")]
    [ProducesResponseType(typeof(CreateQueueEventDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateQueueEventDto>> CreateQueueEvent(CreateQueueEventDto dto)
    {
        var subjectExists = await context.Subjects.AnyAsync(s => s.Id == dto.SubjectId);
        
        if (!subjectExists)
        {
            return NotFound($"Subject with ID {dto.SubjectId} not found.");
        }
        
        var newEvent = new QueueEvent
        {
            Name = dto.Name,
            EventDateTime = dto.EventDateTime,
            SubjectId = dto.SubjectId
        };
        
        context.QueueEvents.Add(newEvent);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(CreateQueueEvent), new { id = newEvent.Id }, newEvent);
    }
    
    

    /// <summary>
    /// Предоставляет группе доступ к предмету.
    /// </summary>
    /// <remarks>
    /// Если связь уже существует, ничего не произойдет.
    /// </remarks>
    /// <param name="dto">DTO с ID группы и предмета.</param>
    [HttpPost("link/group-to-subject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> LinkGroupToSubject(LinkGroupToSubjectDto dto)
    {
        var groupExists = await context.Groups.AnyAsync(g => g.Id == dto.GroupId);
        if (!groupExists)
        {
            return NotFound($"Group with ID {dto.GroupId} not found.");
        }

        var subjectExists = await context.Subjects.AnyAsync(s => s.Id == dto.SubjectId);
        if (!subjectExists)
        {
            return NotFound($"Subject with ID {dto.SubjectId} not found.");
        }

        var linkExists = await context.GroupSubjects
            .AnyAsync(gs => gs.GroupId == dto.GroupId && gs.SubjectId == dto.SubjectId);

        if (linkExists)
        {
            return NoContent();
        }

        var newLink = new GroupSubject
        {
            GroupId = dto.GroupId,
            SubjectId = dto.SubjectId
        };

        context.GroupSubjects.Add(newLink);
        await context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Удаляет связь между группой и предметом (отзывает доступ).
    /// </summary>
    /// <remarks>
    /// Если связь не существует, операция все равно считается успешной (идемпотентность).
    /// </remarks>
    /// <param name="dto">DTO с ID группы и предмета для удаления связи.</param>
    [HttpDelete("unlink/group-from-subject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UnlinkGroupFromSubject([FromQuery] LinkGroupToSubjectDto dto)
    {
        // 1. Находим существующую связь в базе данных
        var linkToDelete = await context.GroupSubjects
            .FirstOrDefaultAsync(gs => gs.GroupId == dto.GroupId && gs.SubjectId == dto.SubjectId);

        if (linkToDelete == null)
        {
            return NoContent();
        }

        context.GroupSubjects.Remove(linkToDelete);
        await context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Удаляет предмет по его ID.
    /// </summary>
    /// <remarks>
    /// ВНИМАНИЕ: Это приведет к каскадному удалению всех связанных заданий и их истории сдачи.
    /// </remarks>
    /// <param name="id">ID предмета для удаления.</param>
    [HttpDelete("subjects/{id:int}/permanently")]
    [Authorize(Policy = "RequireSuperAdminRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteSubject(int id)
    {
        // Находим предмет по ID. Используем FindAsync, так как это поиск по первичному ключу.
        var subject = await context.Subjects.FindAsync(id);

        if (subject == null)
        {
            return NotFound($"Subject with ID {id} not found.");
        }

        context.Subjects.Remove(subject);
        await context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Удаляет группу по ее ID.
    /// </summary>
    /// <remarks>
    /// Пользователи, состоявшие в этой группе, не будут удалены, а станут "без группы" (GroupId = NULL).
    /// </remarks>
    /// <param name="id">ID группы для удаления.</param>
    [HttpDelete("groups/{id:int}/permanently")]
    [Authorize(Policy = "RequireSuperAdminRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var group = await context.Groups.FindAsync(id);

        if (group == null)
        {
            return NotFound($"Group with ID {id} not found.");
        }

        context.Groups.Remove(group);
        await context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Удаляет задание по его ID.
    /// </summary>
    /// <remarks>
    /// ВНИМАНИЕ: Это приведет к каскадному удалению всей истории сдачи этого задания.
    /// </remarks>
    /// <param name="id">ID задания для удаления.</param>
    [HttpDelete("tasks/{id:int}/permanently")]
    [Authorize(Policy = "RequireSuperAdminRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await context.Tasks.FindAsync(id);

        if (task == null)
        {
            return NotFound($"Task with ID {id} not found.");
        }

        context.Tasks.Remove(task);
        await context.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpPut("queue/{eventId}/user/{userId}/status")]
    public async Task<IActionResult> UpdateQueueUserStatus(int eventId, Guid userId, UpdateQueueStatusDto dto)
    {
        var queueUser = await context.QueueUsers
            .Include(qu => qu.CurrentAdmin)
            .FirstOrDefaultAsync(qu => qu.QueueEventId == eventId && qu.UserId == userId);

        if (queueUser == null) return NotFound("User not found in this queue.");

        queueUser.Status = dto.Status;

        await context.SaveChangesAsync();

        string groupName = $"queue-{eventId}";
        var adminName = queueUser.CurrentAdmin != null
            ? $"{queueUser.CurrentAdmin.FirstName} {queueUser.CurrentAdmin.LastName}"
            : null;
        var updateDto = new QueueUserUpdateDto(eventId, userId, dto.Status, adminName);

        await hubContext.Clients.Group(groupName).SendAsync("QueueUserUpdated", updateDto);
        return NoContent();
    }
    
    [HttpGet("get/user/{userId}/task-logs/{taskId}")]
    public async Task<ActionResult<TaskLog>> GetTaskLog(Guid userId, int taskId)
    {
        var res = await context.UserTaskSubmissions
            .Where(ts => ts.TaskId == taskId && ts.UserId == userId)
            .OrderBy(ts => ts.SubmissionDate)
            .Select(ts => new TaskLog(
                ts.Status, 
                ts.Comment, 
                new UserDto(
                    ts.Admin.Id,
                    ts.Admin.FirstName,
                    ts.Admin.LastName,
                    ts.Admin.Color
                ),
                ts.SubmissionDate
            ))
            .ToListAsync();
            
        if (res.Count < 1)
        {
            return NotFound($"No user {userId} logs for such task {taskId} not found.");
        }

        return Ok(res);
    }
    
    
    [HttpGet("get/user/{userId}")]
    public async Task<ActionResult<FullUserDto>> GetUserById(string userId)
    {
        var user =  await context.Users
            .Where(u => u.Id.ToString() == userId)
            .Select(ge => new FullUserDto(userId, ge.FirstName, ge.LastName, ge.TelegramUserId, ge.Color, ge.GitHubUrl))
            .FirstOrDefaultAsync();
        
        if (user == null)
        {
            return NotFound($"User with ID {userId} not found.");
        }
        return Ok(user);
    }
    
    
    [HttpGet("get-all/users/{groupId}")]
    public async Task<ActionResult<List<FullUserDto>>> GetUsersByGroupId(int groupId)
    {
        return await context.Users
            .Where(u => u.Group != null && u.Group.Id == groupId)
            .Select(ge => new FullUserDto(ge.Id.ToString(), ge.FirstName, ge.LastName, ge.TelegramUserId, ge.Color, ge.GitHubUrl))
            .ToListAsync();
    }
}