using FunAndChecks.Models.Enums;

namespace FunAndChecks.DTO;


public record TaskLog(SubmissionStatus Status, string? Comment, UserDto? Admin, DateTime SubmissionDate);
public record LinkGroupToSubjectDto(int GroupId, int SubjectId);

public record ResultUpdateDto(Guid UserId, int TaskId, string NewStatus);

public record QueueUserUpdateDto(int EventId, Guid UserId, QueueUserStatus NewStatus, string? AdminName);
public record CreateSubjectDto(string Name);
public record CreateGroupDto(string Name, int StartYear, int GroupNumber);
public record CreateTaskDto(string Name, string Description, int MaxPoints, int SubjectId);
public record CreateSubmissionDto(Guid UserId, int TaskId, SubmissionStatus Status, string? Comment);
public record CreateQueueEventDto(string Name, DateTime EventDateTime, int SubjectId);
public record UpdateQueueStatusDto(QueueUserStatus Status);

public record FullUserDto(string Id, string FirstName, string LastName, long? TelegramUserId, string? Color, string? GitHubUrl);
