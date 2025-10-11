using System.Text.Json.Serialization;

namespace AdminUI.Models;

[method: JsonConstructor]
public class QueueParticipantDetailDto(
    Guid userId,
    string firstName,
    string lastName,
    string groupName,
    int totalPoints,
    QueueUserStatus status,
    string color,
    string? checkingByAdminName)
{
    public string? CheckingByAdminName { get; } = checkingByAdminName;
    public string Color { get; } = color;
    public int TotalPoints { get; } = totalPoints;
    public string GroupName { get; } = groupName;
    public string LastName { get; } = lastName;
    public string FirstName { get; } = firstName;
    public Guid UserId { get; } = userId;

    // Это свойство должно иметь setter, чтобы мы могли обновлять его в диалоге
    public QueueUserStatus Status { get; set; } = status;
}

public record QueueDetailsDto(
    int EventId,
    string EventName,
    string SubjectName,
    int SubjectId,
    DateTime EventDateTime,
    List<QueueParticipantDetailDto> Participants
);

public record TaskResultDto(int TaskId, string TaskName, string Status);

public record ResultsTableRowDto(Guid UserId, string FullName, string GroupName,
    List<TaskResultDto> TaskResults);
    
public record EventDto(int EventId, string EventName, DateTime EventDateTime);
public class SubjectDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class GroupDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class TaskDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Points { get; set; }
}


public class TaskUserDto(int id, string name, string description, int points, SubmissionStatus status)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int Points { get; set; } = points;
    public SubmissionStatus Status { get; set; } = status;
}

public record TaskHeaderDto(int TaskId, string TaskName);
public record SubjectResultsDto(long SubjectId, string SubjectName, List<TaskHeaderDto> TaskHeaders, List<UserResultDto> UserResults);

public record QueueEventDto(int Id, string Name, DateTime EventDateTime);

public record UserDto(Guid Id, string Name, string LastName, string? Color);


/// <summary>
/// Представляет одну ячейку в таблице результатов.
/// </summary>
public record ResultCellDto(
    string DisplayValue, // Что будет написано в ячейке ("+", "A", "Б", "")
    string? AdminColor,  // Цвет фона ячейки (например, "#FF5733")
    SubmissionStatus Status
);

// Обновляем UserResultDto, чтобы он использовал новый DTO для ячеек.
// Словарь теперь будет [TaskId, ResultCellDto].
public record UserResultDto(
    Guid UserId,
    string FullName,
    string GroupName,
    int TotalPoints,
    // Словарь [TaskId, Status]
    Dictionary<int, ResultCellDto> Results

);
