using System.Security.Claims;
using FunAndChecks.Data;
using FunAndChecks.DTO;
using FunAndChecks.Hub;
using FunAndChecks.Models;
using FunAndChecks.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FunAndChecks.Controllers;


/// <summary>
/// Запросы доступные пользователю после авторизации
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(
    UserManager<User> userManager,
    IHubContext<QueueHub> hubContext,
    ApplicationDbContext context)
    : ControllerBase
{
    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) 
                                                  ?? throw new InvalidOperationException("Invalid claim"));


    [HttpGet("me")]
    public async Task<ActionResult<UserInfoDto>> GetMe()
    {
        var user = await userManager.Users
            .Include(u => u.Group)
            .FirstOrDefaultAsync(u => u.Id.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (user == null) return NotFound();

        return Ok(new UserInfoDto(user.Id, user.FirstName, user.LastName, user.Email, user.Group?.Name));
    }
    

    [HttpPut("me/link-telegram")]
    public async Task<IActionResult> LinkTelegramAccount([FromBody] LinkTelegramDto dto)
    {
        if (User.IsInRole("Admin"))
        {
            return Forbid("Administrators cannot link");
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized();
        }

        var currentUser = await userManager.FindByIdAsync(userIdString);
        if (currentUser == null)
        {
            return NotFound("Current user not found.");
        }

        var isTelegramIdTaken = await userManager.Users
            .AnyAsync(u => u.TelegramUserId == dto.TelegramId && u.Id != currentUser.Id);

        if (isTelegramIdTaken)
        {
            return Conflict("This Telegram account is already linked to another user.");
        }

        currentUser.TelegramUserId = dto.TelegramId;
        var result = await userManager.UpdateAsync(currentUser);

        if (result.Succeeded)
        {
            return NoContent();
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("queue/{eventId}/join")]
    public async Task<IActionResult> JoinQueue(int eventId)
    {
        if (User.IsInRole("Admin"))
        {
            return Forbid("Administrators cannot join the queue.");
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }


        var userGroupId = user.GroupId;
        if (userGroupId == null)
        {
            return Forbid("You are not assigned to any group.");
        }


        var isGroupAllowed = await context.QueueEvents
            .Where(qe => qe.Id == eventId)
            .AnyAsync(qe => qe.Subject.GroupSubjects.Any(gs => gs.GroupId == userGroupId.Value));

        if (!isGroupAllowed)
        {
            return Forbid("Your group does not have access to the subject of this event.");
        }

        var alreadyInQueue = await context.QueueUsers
            .AnyAsync(qu => qu.QueueEventId == eventId && qu.UserId == userId);
        if (alreadyInQueue) return Conflict("User already in queue.");

        var queueUser = new QueueUser
        {
            QueueEventId = eventId,
            UserId = userId,
            JoinTime = DateTime.UtcNow,
            Status = QueueUserStatus.Waiting
        };

        context.QueueUsers.Add(queueUser);
        await context.SaveChangesAsync();

        string groupName = $"queue-{eventId}";
        var updateDto = new QueueUserUpdateDto(eventId, userId, QueueUserStatus.Waiting, null);

        await hubContext.Clients.Group(groupName).SendAsync("QueueUserUpdated", updateDto);


        return Ok();
    }

    /// <summary>
    /// Получает список предметов, доступных для группы текущего пользователя.
    /// </summary>
    [HttpGet("me/subjects")]
    [ProducesResponseType(typeof(List<SubjectDto>), 200)]
    public async Task<ActionResult<List<SubjectDto>>> GetMySubjects()
    {
        var userId = GetCurrentUserId();
        var user = await context.Users.FindAsync(userId);

        if (user?.GroupId == null)
        {
            // Если пользователь не в группе, у него нет доступных предметов
            return Ok(new List<SubjectDto>());
        }

        var subjects = await context.Subjects
            .Where(s => s.GroupSubjects.Any(gs => gs.GroupId == user.GroupId))
            .Select(s => new SubjectDto(s.Id, s.Name))
            .ToListAsync();

        return Ok(subjects);
    }
    /// <summary>
    /// Получает список активных событий в очереди, на которые записан текущий пользователь.
    /// </summary>
    [HttpGet("me/queue-events")]
    [ProducesResponseType(typeof(List<QueueEventDto>), 200)]
    public async Task<ActionResult<List<QueueEventDto>>> GetMyQueueEvents()
    {
        var userId = GetCurrentUserId();
        var user = await context.Users.FindAsync(userId);

        if (user?.GroupId == null)
        {
            return Ok(new List<QueueEventDto>());
        }

        var now = DateTime.UtcNow - TimeSpan.FromDays(1);
        var availableEvents = await context.QueueEvents
            .Where(qe => qe.EventDateTime > now)
            .Where(qe => qe.Subject.GroupSubjects.Any(gs => gs.GroupId == user.GroupId))
            .Where(qe => qe.Participants.Any(p => p.UserId == userId))
            .OrderBy(qe => qe.EventDateTime)
            .Select(qe => new QueueEventDto(qe.Id, qe.Name, qe.EventDateTime))
            .ToListAsync();

        return Ok(availableEvents);
    }

    /// <summary>
    /// Получает список активных событий в очереди, на которые может записаться текущий пользователь.
    /// </summary>
    [HttpGet("me/available-queue-events")]
    [ProducesResponseType(typeof(List<QueueEventDto>), 200)]
    public async Task<ActionResult<List<QueueEventDto>>> GetMyAvailableQueueEvents()
    {
        var userId = GetCurrentUserId();
        var user = await context.Users.FindAsync(userId);

        if (user?.GroupId == null)
        {
            return Ok(new List<QueueEventDto>());
        }

        var now = DateTime.UtcNow - TimeSpan.FromDays(1);
        var availableEvents = await context.QueueEvents
            .Where(qe => qe.EventDateTime > now)
            .Where(qe => qe.Subject.GroupSubjects.Any(gs => gs.GroupId == user.GroupId))
            .OrderBy(qe => qe.EventDateTime)
            .Select(qe => new QueueEventDto(qe.Id, qe.Name, qe.EventDateTime))
            .ToListAsync();

        return Ok(availableEvents);
    }

    /// <summary>
    /// Получает полную информацию о группе, в которой состоит текущий пользователь.
    /// </summary>
    [HttpGet("me/group")]
    [ProducesResponseType(typeof(GroupDto), 200)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupDto>> GetMyGroup()
    {
        var userId = GetCurrentUserId();

        var group = await context.Users
            .Where(u => u.Id == userId && u.Group != null)
            .Select(u => new GroupDto(u.Group!.Id, u.Group.Name, u.Group.StartYear, u.Group.GroupNumber))
            .FirstOrDefaultAsync();

        if (group == null)
        {
            return NotFound("You are not assigned to any group, or the group does not exist.");
        }

        return Ok(group);
    }

    /// <summary>
    /// Получает детализированные результаты текущего пользователя по конкретному предмету.
    /// </summary>
    [HttpGet("me/results/subject/{subjectId}")]
    [ProducesResponseType(typeof(UserSubjectResultsDto), 200)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserSubjectResultsDto>> GetMyResultsForSubject(int subjectId)
    {
        var userId = GetCurrentUserId();

        var subject = await context.Subjects
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null) return NotFound("Subject not found.");

        var userSubmissionsForSubject = await context.UserTaskSubmissions
            .Include(s => s.Admin)
            .Where(s => s.UserId == userId && s.Task.SubjectId == subjectId)
            .ToListAsync();

        var taskResults = new List<UserTaskResultDto>();

        foreach (var task in subject.Tasks)
        {
            var submissionsForTask = userSubmissionsForSubject
                .Where(s => s.TaskId == task.Id)
                .OrderByDescending(s => s.SubmissionDate)
                .ToList();

            var latestSubmission = submissionsForTask.FirstOrDefault();
            var currentStatus = latestSubmission?.Status ?? SubmissionStatus.NotSubmitted;

            List<SubmissionLogDto>? history = null;

            if (currentStatus == SubmissionStatus.Rejected)
            {
                history = submissionsForTask.Select(s => new SubmissionLogDto(
                    s.Status,
                    s.Comment,
                    s.SubmissionDate,
                    $"{s.Admin.FirstName} {s.Admin.LastName}"
                )).ToList();
            }

            taskResults.Add(new UserTaskResultDto(
                task.Id,
                task.Name,
                currentStatus,
                task.MaxPoints,
                history
            ));
        }

        int totalPointsEarned = taskResults
            .Where(tr => tr.CurrentStatus == SubmissionStatus.Accepted)
            .Sum(tr => tr.MaxPoints);

        int maxPointsPossible = subject.Tasks.Sum(t => t.MaxPoints);

        var result = new UserSubjectResultsDto(
            subject.Id,
            subject.Name,
            totalPointsEarned,
            maxPointsPossible,
            taskResults
        );

        return Ok(result);
    }
}