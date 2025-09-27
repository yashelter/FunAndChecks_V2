using FunAndChecks.DTO;
using FunAndChecks.Models.Enums;

namespace TelegramBot.Services.ApiClient;

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
    
    Task<bool> RegisterUser (RegisterUserDto dto);
    Task<bool> JoinQueue (long telegramId, int queueId);
    
    Task<List<GroupDto>?> GetAllGroups();
    Task<List<SubjectDto>?> GetAllSubjects();
    Task<List<SubjectDto>?> GetMySubjects(long telegramId);
    Task<List<QueueEventDto>?> GetMyQueueEvents(long telegramId);
    Task<List<TaskDto>?> GetAllTasks(int subjectId);
    Task<List<TaskUserDto>?> GetAllUserTasks(string userId, int subjectId);
    Task<List<QueueEventDto>?> GetAllQueueEvents();
    
    
    
}