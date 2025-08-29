using FunAndChecks.Data;
using FunAndChecks.DTO;
using FunAndChecks.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FunAndChecks.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublicController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PublicController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("queue/{eventId}")]
    public async Task<ActionResult<QueueStateDto>> GetQueueState(int eventId)
    {
        var queueEvent = await _context.QueueEvents
            .Include(qe => qe.Participants).ThenInclude(p => p.User)
            .Include(qe => qe.Participants).ThenInclude(p => p.CurrentAdmin)
            .Where(qe => qe.Id == eventId)
            .Select(qe => new QueueStateDto(
                qe.Id,
                qe.Name,
                qe.EventDateTime,
                qe.Participants.OrderBy(p => p.JoinTime).Select(p => new QueueParticipantDto(
                    p.UserId,
                    $"{p.User.FirstName} {p.User.LastName}",
                    p.Status,
                    p.JoinTime,
                    p.CurrentAdmin != null ? $"{p.CurrentAdmin.FirstName} {p.CurrentAdmin.LastName}" : null
                )).ToList()
            ))
            .FirstOrDefaultAsync();

        if (queueEvent == null) return NotFound();
        return Ok(queueEvent);
    }

    [HttpGet("results-table")]
    public async Task<ActionResult<List<ResultsTableRowDto>>> GetResultsTable()
    {
        // Этот запрос может быть тяжелым. Для продакшена стоит рассмотреть оптимизацию.
        var users = await _context.Users
            .Include(u => u.Group)
            .Include(u => u.Submissions)
            .ThenInclude(s => s.Task)
            .ToListAsync();

        var allTasks = await _context.Tasks.ToListAsync();

        var result = users.Select(user => new ResultsTableRowDto(
            user.Id,
            $"{user.FirstName} {user.LastName}",
            user.Group?.Name ?? "N/A",
            allTasks.Select(task =>
            {
                var latestSubmission = user.Submissions
                    .Where(s => s.TaskId == task.Id)
                    .OrderByDescending(s => s.SubmissionDate)
                    .FirstOrDefault();

                return new TaskResultDto(
                    task.Id,
                    task.Name,
                    latestSubmission?.Status.ToString() ?? SubmissionStatus.NotSubmitted.ToString()
                );
            }).ToList()
        )).ToList();

        return Ok(result);
    }


    [HttpGet("results/subject/{subjectId}")]
    public async Task<ActionResult<SubjectResultsDto>> GetResultsForSubject(int subjectId)
    {
        var subject = await _context.Subjects
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject == null) return NotFound();

        var users = await _context.Users
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "User")) // todo: this filter 
            .Include(u => u.Group)
            .Include(u => u.Submissions)
            .ToListAsync();

        var taskHeaders = subject.Tasks.Select(t => new TaskHeaderDto(t.Id, t.Name)).ToList();

        var userResults = users.Select(user => new UserResultDto(
            user.Id,
            $"{user.FirstName} {user.LastName}",
            user.Group?.Name ?? "N/A",
            subject.Tasks.ToDictionary(
                task => task.Id,
                task => user.Submissions
                    .Where(s => s.TaskId == task.Id)
                    .OrderByDescending(s => s.SubmissionDate)
                    .FirstOrDefault()?.Status.ToString() ?? "NotSubmitted"
            )
        )).ToList();

        var result = new SubjectResultsDto(subjectId, subject.Name, taskHeaders, userResults);
        return Ok(result);
    }


    [HttpGet("get/subject/{subjectId}")]
    public async Task<ActionResult<SubjectDto>> GetSubject(int subjectId)
    {
        var subject = await _context.Subjects
            .Where(s => s.Id == subjectId)
            .Select(s => new SubjectDto(s.Id, s.Name))
            .FirstOrDefaultAsync();

        if (subject == null) return NotFound();

        return Ok(subject);
    }


    [HttpGet("queue/get/{eventId}/details")]
    [ProducesResponseType(typeof(QueueDetailsDto), 200)]
    public async Task<ActionResult<QueueDetailsDto>> GetQueueDetails(int eventId)
    {
        var queueDetails = await _context.QueueEvents
            .Where(qe => qe.Id == eventId)
            .Select(qe => new QueueDetailsDto(
                qe.Id,
                qe.Name,
                qe.Subject.Name,
                qe.EventDateTime,
                qe.Participants
                    .OrderBy(p => p.JoinTime)
                    .Select(p => new QueueParticipantDetailDto(
                        p.UserId,
                        p.User.FirstName,
                        p.User.LastName,
                        p.User.Group.Name,
                        p.User.Submissions
                            .Where(s => s.Status == SubmissionStatus.Accepted)
                            .Sum(s => s.Task.MaxPoints),
                        p.Status,
                        p.User.Color ?? "#000000",
                        p.CurrentAdmin != null ? $"{p.CurrentAdmin.FirstName} {p.CurrentAdmin.LastName}" : null
                    )).ToList()
            ))
            .FirstOrDefaultAsync();

        if (queueDetails == null)
        {
            return NotFound("Queue event not found.");
        }

        return Ok(queueDetails);
    }



    [HttpGet("get/group/{groupId}")]
    public async Task<ActionResult<GroupDto>> GetGroup(int groupId)
    {
        var subject = await _context.Groups
            .Where(s => s.Id == groupId)
            .Select(s => new GroupDto(s.Id, s.Name, s.StartYear, s.GroupNumber))
            .FirstOrDefaultAsync();

        if (subject == null) return NotFound();

        return Ok(subject);
    }


    /// <summary>
    /// Возвращает список всех доступных учебных групп.
    /// </summary>
    /// <remarks>
    /// Этот эндпоинт не требует авторизации и используется, например,
    /// при регистрации нового пользователя, чтобы предоставить ему выбор группы.
    /// </remarks>
    /// <returns>Список групп с их ID и названиями.</returns>
    /// <response code="200">Возвращает список групп.</response>
    [HttpGet("get-all/groups")]
    [ProducesResponseType(typeof(IEnumerable<GroupDto>), 200)]
    public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroups()
    {
        var groups = await _context.Groups
            .OrderBy(g => g.Name)
            .Select(g => new GroupDto(g.Id, g.Name, g.StartYear, g.GroupNumber))
            .ToListAsync();
        return Ok(groups);
    }


    [HttpGet("get-all/subjects")]
    public async Task<ActionResult<List<SubjectDto>>> GetSubjects()
    {
        return await _context.Subjects
            .Select(s => new SubjectDto(s.Id, s.Name))
            .ToListAsync();
    }


    [HttpGet("get-all/queue/events")]
    public async Task<ActionResult<List<QueueEventDto>>> GetQueueEvents()
    {
        return await _context.QueueEvents
            .Select(qe => new QueueEventDto(qe.Id, qe.Name, qe.EventDateTime))
            .ToListAsync();
    }


    [HttpGet("get-all/users/{groupId}")]
    public async Task<ActionResult<List<UserDto>>> GetUsersByGroupId(int groupId)
    {
        return await _context.Users
            .Where(u => u.Group != null && u.Group.Id == groupId)
            .Select(ge => new UserDto(ge.Id.ToString(), ge.FirstName, ge.LastName, ge.Color))
            .ToListAsync();
    }

}