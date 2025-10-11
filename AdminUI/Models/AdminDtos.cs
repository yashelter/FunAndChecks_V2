namespace AdminUI.Models;


public record TaskLog(SubmissionStatus Status, string? Comment, UserDto? Admin, DateTime SubmissionDate);

public record ResultUpdateDto(Guid UserId, int TaskId, string NewStatus);

public record QueueUserUpdateDto(int EventId, Guid UserId, QueueUserStatus NewStatus, string? AdminName);
public record CreateSubmissionDto(Guid UserId, int TaskId, SubmissionStatus Status, string? Comment);
public record UpdateQueueStatusDto(QueueUserStatus Status);

public record FullUserDto(string Id, string FirstName, string LastName, long? TelegramUserId, string? Color, string? GitHubUrl);

public class CreateSubjectDto
{
    public string Name { get; set; }
}

// FunAndChecks.AdminUI/Models/CreateGroupDto.cs
public class CreateGroupDto
{
    public string Name { get; set; }
    public int StartYear { get; set; } = DateTime.Now.Year;
    public int GroupNumber { get; set; }
}

// FunAndChecks.AdminUI/Models/CreateTaskDto.cs
public class CreateTaskDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int MaxPoints { get; set; }
    public int SubjectId { get; set; }
}

public class CreateQueueEventDto
{
    public string Name { get; set; }
    public DateTime EventDateTime { get; set; }
    public int SubjectId { get; set; }
}

public class LinkGroupToSubjectDto
{
    public int GroupId { get; set; }
    public int SubjectId { get; set; }
}


