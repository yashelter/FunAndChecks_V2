namespace AdminUI.Models;


public record UserInfoDto(
    Guid Id, string FirstName, string LastName, string Email, string? GroupName);

public record LinkTelegramDto(long TelegramId);

/// <summary>
/// Представляет одну попытку сдачи задания.
/// </summary>
public record SubmissionLogDto(
    SubmissionStatus Status,
    string? Comment,
    DateTime SubmissionDate,
    string AdminName // Имя проверившего админа
);

/// <summary>
/// Представляет одну задачу в списке результатов пользователя по предмету.
/// </summary>
public record UserTaskResultDto(
    int TaskId,
    string TaskName,
    SubmissionStatus CurrentStatus,
    int MaxPoints,
    // Список будет заполнен только для задач со статусом Rejected
    List<SubmissionLogDto>? SubmissionHistory 
);

/// <summary>
/// Представляет итоговые результаты пользователя по одному предмету.
/// </summary>
public record UserSubjectResultsDto(
    int SubjectId,
    string SubjectName,
    int TotalPointsEarned, // Сумма полученных баллов
    int MaxPointsPossible, // Максимально возможная сумма баллов по предмету
    List<UserTaskResultDto> TaskResults
);