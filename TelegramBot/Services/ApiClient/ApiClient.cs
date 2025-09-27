using FunAndChecks.DTO;
using FunAndChecks.Models.Enums;
using TelegramBot.Services.StateStorage;

namespace TelegramBot.Services.ApiClient;

public class ApiClient(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<ApiClient> logger, // TODO: log everything
    ILogger<ApiRequestsWrapper> baseLogger,
    BotStateService botStateService)
    : ApiRequestsWrapper(configuration,
            baseLogger,
            botStateService,
            () => httpClientFactory.CreateClient("ApiV1")), 
        IApiClient
{
    
    public async Task<UserInfoDto?> GetMyInfoAsync(long userId)
    {
        return await GetWithAuthAsync<UserInfoDto>(userId, "/api/users/me");
    }
    
    public async Task<SubjectDto?> GetSubject(int id)
    {
        return await GetAsync<SubjectDto>($"/api/public/get/subject/{id}");
    }

    public async Task<QueueDetailsDto?> GetQueueDetails(int id)
    {
        return await GetAsync<QueueDetailsDto>($"/api/public/get/queue/{id}");
    }

    public async Task<GroupDto?> GetGroup(int id)
    {
        return await GetAsync<GroupDto>($"/api/public/get/group/{id}");
    }

    public async Task<bool> RegisterUser(RegisterUserDto dto)
    {
        return await PostWithoutAuthAsync<RegisterUserDto>("/api/auth/register", dto);
    }

    public async Task<bool> JoinQueue(long telegramId, int queueId)
    {
        return await PostWithAuthAsync(telegramId, $"/api/users/queue/{queueId}/join");
    }

    public async Task<QueueDetailsDto?> GetQueue(int id)
    {
        return await GetAsync<QueueDetailsDto>("/api/public/queue/{id}");
    }


    public async Task<List<GroupDto>?> GetAllGroups()
    {
        return await GetAsync<List<GroupDto>>("/api/public/get-all/groups");
    }

    public async Task<List<SubjectDto>?> GetAllSubjects()
    {
        return await GetAsync<List<SubjectDto>>("/api/public/get-all/subjects");
    }

    public async Task<List<SubjectDto>?> GetMySubjects(long telegramId)
    {
        return await GetWithAuthAsync<List<SubjectDto>>( telegramId, "/api/users/me/subjects");
    }

    public async Task<List<QueueEventDto>?> GetMyQueueEvents(long telegramId)
    {
        return await GetWithAuthAsync<List<QueueEventDto>>( telegramId, "/api/users/me/available-queue-events");
    }

    public async Task<List<TaskDto>?> GetAllTasks(int subjectId)
    {
        return await GetAsync<List<TaskDto>>($"/api/public/get-all/tasks/{subjectId}");
    }

    public async Task<List<TaskUserDto>?> GetAllUserTasks(string userId, int subjectId)
    {
        return await GetAsync<List<TaskUserDto>>($"/api/public/subjects/{subjectId}/tasks/user/{userId}");
    }
    
    public async Task<List<QueueEventDto>?> GetAllQueueEvents()
    {
        return await GetAsync<List<QueueEventDto>>("/api/public/get-all/queue/events");
    }
}