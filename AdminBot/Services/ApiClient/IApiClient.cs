using FunAndChecks.DTO;

namespace AdminBot.Services.ApiClient;

public interface IApiClient
{
    /// <summary>
    /// Пытается войти в систему беспарольно, используя Telegram ID.
    /// </summary>
    /// <returns>JWT токен в случае успеха, иначе null.</returns>
    Task<string?> TelegramLoginAsync(long telegramId);

    Task<SubjectDto?> GetSubject(int id);
    
    Task<QueueDetailsDto?> GetQueueDetails(int id);
    Task<GroupDto?> GetGroup(int id);
    Task<FullUserDto?> GetUser(long userId, string id);
    
    Task<List<GroupDto>?> GetAllGroups();
    Task<List<SubjectDto>?> GetAllSubjects();
    Task<List<FullUserDto>?> GetAllUsers(long userId, int groupId);
    Task<List<QueueEventDto>?> GetAllQueueEvents();
    
    Task<SubjectDto?> CreateNewSubject(long userId, string name);
    Task<GroupDto?> CreateNewGroup(long userId, string name, int groupNumber, int startYear);
    Task<CreateTaskDto?> CreateNewTask(long userId, int subjectId, string name, int maxPoints,
        string description = "none");
    
    Task<CreateQueueEventDto?> CreateQueueEvent(long userId,  string name, DateTime? eventTime, int subjectId);

}