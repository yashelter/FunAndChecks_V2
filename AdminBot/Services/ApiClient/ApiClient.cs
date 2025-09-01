using AdminBot.Services.StateStorage;
using FunAndChecks.DTO;
using FunAndChecks.Models.Enums;

namespace AdminBot.Services.ApiClient;

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

    public async Task<QueueDetailsDto?> GetQueue(int id)
    {
        return await GetAsync<QueueDetailsDto>("/api/public/queue/{id}");
    }

    public async Task<FullUserDto?> GetUser(long adminId, string id)
    {
        return await GetWithAuthAsync<FullUserDto>(adminId,$"/api/admin/get/user/{id}");
    }

    public async Task<List<GroupDto>?> GetAllGroups()
    {
        return await GetAsync<List<GroupDto>>("/api/public/get-all/groups");
    }

    public async Task<List<SubjectDto>?> GetAllSubjects()
    {
        return await GetAsync<List<SubjectDto>>("/api/public/get-all/subjects");
    }

    public async Task<List<TaskDto>?> GetAllTasks(int subjectId)
    {
        return await GetAsync<List<TaskDto>>($"/api/public/get-all/tasks/{subjectId}");
    }

    public async Task<List<TaskUserDto>?> GetAllUserTasks(string userId, int subjectId)
    {
        return await GetAsync<List<TaskUserDto>>($"/api/public/subjects/{subjectId}/tasks/user/{userId}");
    }

    public async Task<List<TaskLog>?> GetAllTaskLogs(long adminId, string userId, int taskId)
    {
        try
        {
            return await GetWithAuthAsync<List<TaskLog>>(adminId, $"/api/admin/get/user/{userId}/task-logs/{taskId}");
        }
        catch (Exception ex)
        {
            return null;
        }
    }


    public async Task<List<FullUserDto>?> GetAllUsers(long adminId, int groupId)
    {
        return await GetWithAuthAsync<List<FullUserDto>>(adminId,$"/api/admin/get-all/users/{groupId}");
    }

    public async Task<List<QueueEventDto>?> GetAllQueueEvents()
    {
        return await GetAsync<List<QueueEventDto>>("/api/public/get-all/queue/events");
    }

    public async Task CreateSubmission(long adminId, string userId, int taskId, SubmissionStatus status, string comment)
    {
        var tr = Guid.TryParse(userId, out Guid id);
        var requestDto = new CreateSubmissionDto(id, taskId, status, comment);
        
        var result = await PostWithAuthAsync<CreateSubmissionDto>(
            adminId, 
            "/api/admin/create/submission",
            requestDto
        );

        if (!result)
        {
            throw new InvalidOperationException("Something went wrong in LinkGroupToSubject");
        }
    }


    public async Task<SubjectDto?> CreateNewSubject(long adminId, string name)
    {
        var requestDto = new CreateSubjectDto(name);
        
        return await PostWithAuthAsync<CreateSubjectDto, SubjectDto>(
            adminId, 
            "/api/admin/create/subject",
            requestDto
        );
    }
    
    public async Task<GroupDto?> CreateNewGroup(long adminId, string name, int groupNumber, int startYear)
    {
        var requestDto = new CreateGroupDto(name, startYear, groupNumber);
        
        return await PostWithAuthAsync<CreateGroupDto, GroupDto>(
            adminId, 
            "/api/admin/create/group",
            requestDto
        );
    }
    
    public async Task<CreateTaskDto?> CreateNewTask(long adminId, int subjectId, string name, int maxPoints,
        string description = "none")
    {
        var requestDto = new CreateTaskDto(name, description, maxPoints, subjectId);
        
        return await PostWithAuthAsync<CreateTaskDto, CreateTaskDto>(
            adminId, 
            "/api/admin/create/task",
            requestDto
        );
    }

    public async Task<CreateQueueEventDto?> CreateQueueEvent(long adminId, string name, DateTime? eventTime, int subjectId)
    {
        if (!eventTime.HasValue)
        {
            throw new ArgumentNullException(nameof(eventTime));
        }

        var requestDto = new CreateQueueEventDto(name, (DateTime) eventTime, subjectId);
        
        return await PostWithAuthAsync<CreateQueueEventDto, CreateQueueEventDto>(
            adminId, 
            "/api/admin/create/queue-event",
            requestDto
        );
    }

    public async Task LinkGroupToSubject(long adminId, int groupId, int subjectId)
    {
        var requestDto = new LinkGroupToSubjectDto(groupId, subjectId);
        
        var result = await PostWithAuthAsync<LinkGroupToSubjectDto>(
            adminId, 
            "/api/admin/link/group-to-subject",
            requestDto
        );

        if (!result)
        {
            throw new InvalidOperationException("Something went wrong in LinkGroupToSubject");
        }
    }
}