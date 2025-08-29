using FunAndChecks.DTO;

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
        return await GetAsync<QueueDetailsDto>($"/api/public/queue/{id}");
    }

    public async Task<GroupDto?> GetGroup(int id)
    {
        return await GetAsync<GroupDto>($"/api/public/get/group/{id}");
    }

    public async Task<FullUserDto?> GetUser(long userId, string id)
    {
        return await GetWithAuthAsync<FullUserDto>(userId,$"/api/admin/get/user/{id}");
    }

    public async Task<List<GroupDto>?> GetAllGroups()
    {
        return await GetAsync<List<GroupDto>>("/api/public/get-all/groups");
    }

    public async Task<List<SubjectDto>?> GetAllSubjects()
    {
        return await GetAsync<List<SubjectDto>>("/api/public/get-all/subjects");
    }


    public async Task<List<FullUserDto>?> GetAllUsers(long userId, int groupId)
    {
        return await GetWithAuthAsync<List<FullUserDto>>(userId,$"/api/admin/get-all/users/{groupId}");
    }

    public async Task<List<QueueEventDto>?> GetAllQueueEvents()
    {
        return await GetAsync<List<QueueEventDto>>("/api/public/get-all/queue/events");
    }


    public async Task<SubjectDto?> CreateNewSubject(long userId, string name)
    {
        var requestDto = new CreateSubjectDto(name);
        
        return await PostWithAuthAsync<CreateSubjectDto, SubjectDto>(
            userId, 
            "/api/admin/create/subject",
            requestDto
        );
    }
    
    public async Task<GroupDto?> CreateNewGroup(long userId, string name, int groupNumber, int startYear)
    {
        var requestDto = new CreateGroupDto(name, startYear, groupNumber);
        
        return await PostWithAuthAsync<CreateGroupDto, GroupDto>(
            userId, 
            "/api/admin/create/group",
            requestDto
        );
    }
    
    public async Task<CreateTaskDto?> CreateNewTask(long userId, int subjectId, string name, int maxPoints,
        string description = "none")
    {
        var requestDto = new CreateTaskDto(name, description, maxPoints, subjectId);
        
        return await PostWithAuthAsync<CreateTaskDto, CreateTaskDto>(
            userId, 
            "/api/admin/create/task",
            requestDto
        );
    }

    public async Task<CreateQueueEventDto?> CreateQueueEvent(long userId, string name, DateTime? eventTime, int subjectId)
    {
        ArgumentNullException.ThrowIfNull(eventTime);
        var requestDto = new CreateQueueEventDto(name, (DateTime) eventTime, subjectId);
        
        return await PostWithAuthAsync<CreateQueueEventDto, CreateQueueEventDto>(
            userId, 
            "/api/admin/create/queue-event",
            requestDto
        );
    }
}