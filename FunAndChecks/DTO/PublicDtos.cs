using FunAndChecks.Models.Enums;

namespace FunAndChecks.DTO;

public record QueueParticipantDetailDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string GroupName,
    int TotalPoints,
    QueueUserStatus Status,
    string Color,
    string? CheckingByAdminName
);


public record QueueDetailsDto(
    int EventId,
    string EventName,
    string SubjectName,
    DateTime EventDateTime,
    List<QueueParticipantDetailDto> Participants
);

public record TaskResultDto(int TaskId, string TaskName, string Status);

public record ResultsTableRowDto(Guid UserId, string FullName, string GroupName,
    List<TaskResultDto> TaskResults);
public record QueueParticipantDto(
    Guid UserId, string FullName, QueueUserStatus Status, DateTime JoinTime, string? CheckingByAdmin);

public record QueueStateDto(
    int EventId, string EventName, DateTime EventDateTime, List<QueueParticipantDto> Participants);
    
public record EventDto(
    int EventId, string EventName, DateTime EventDateTime);


public record GroupDto(int Id, string Name, int startYear, int groupNumber);
public record SubjectDto(int Id, string Name);
public record TaskHeaderDto(int TaskId, string TaskName);
public record UserResultDto(Guid UserId, string FullName, string GroupName, Dictionary<int, string> Results); // Словарь [TaskId, Status]
public record SubjectResultsDto(long SubjectId, string SubjectName, List<TaskHeaderDto> TaskHeaders, List<UserResultDto> UserResults);

public record QueueEventDto(int Id, string Name, DateTime EventDateTime);

public record UserDto(string Id, string Name, string LastName, string? Color);
