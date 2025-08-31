using FunAndChecks.DTO;
using FunAndChecks.Models.Enums;

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
    Task<FullUserDto?> GetUser(long adminId, string id);
    
    
    Task<List<GroupDto>?> GetAllGroups();
    Task<List<SubjectDto>?> GetAllSubjects();
    Task<List<TaskDto>?> GetAllTasks(int subjectId);
    Task<List<TaskUserDto>?> GetAllUserTasks(string userId, int subjectId);
    Task<List<TaskLog>?> GetAllTaskLogs(long adminId, string userId, int taskId);
    Task<List<FullUserDto>?> GetAllUsers(long adminId, int groupId);
    Task<List<QueueEventDto>?> GetAllQueueEvents();
    
    
    Task CreateSubmission(long adminId, string userId, int taskId, SubmissionStatus status, string comment);
    Task<SubjectDto?> CreateNewSubject(long adminId, string name);
    Task<GroupDto?> CreateNewGroup(long adminId, string name, int groupNumber, int startYear);
    Task<CreateTaskDto?> CreateNewTask(long adminId, int subjectId, string name, int maxPoints,
        string description = "none");
    
    Task<CreateQueueEventDto?> CreateQueueEvent(long adminId,  string name, DateTime? eventTime, int subjectId);
    
    Task LinkGroupToSubject(long adminId, int groupId, int subjectId);

}