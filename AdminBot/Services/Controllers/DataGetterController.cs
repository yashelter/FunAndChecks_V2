using AdminBot.Services.ApiClient;
using AdminBot.Services.Keyboard;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.Services.Controllers;

public static class DataGetterController
{
    public static async Task<InlineKeyboardMarkup> GetAllUserTasks(string userId, int subjectId, IApiClient apiClient, int page = 0)
    {
        var tasks = await apiClient.GetAllUserTasks(userId, subjectId);
        if (tasks is null) throw new InvalidDataException("No subjects found");
        
        var res = tasks.Wrap();
        return KeyboardGenerator.GenerateKeyboardPage(res, $"tasks_picker[{subjectId}]", page, lineSize: 1);
    }
    
    public static async Task<List<string>?> GetAllUserTaskLogs(long adminId,
        string userId,
        int taskId,
        IApiClient apiClient,
        int page = 0)
    {
        var tasks = await apiClient.GetAllTaskLogs(adminId, userId, taskId);
        var res = tasks.Wrap();
        return res;
    }
    
    
    public static async Task<InlineKeyboardMarkup> GetAllSubjects(IApiClient apiClient, int page = 0)
    {
        var subjects = await apiClient.GetAllSubjects();
        if (subjects is null) throw new InvalidDataException("No subjects found");
        
        var res = subjects.Wrap();
        
        return KeyboardGenerator.GenerateKeyboardPage(res, "subject_picker", page, lineSize: 1);
    }
    
    
    public static async Task<InlineKeyboardMarkup> GetAllQueueEvents(IApiClient apiClient, int page = 0)
    {
        var res = await apiClient.GetAllQueueEvents();
        var events = res.Wrap();
        
        return KeyboardGenerator.GenerateKeyboardPage(events, "all_queues_getter", page, lineSize: 1);
    }
    
    public static async Task<InlineKeyboardMarkup> GetAllGroups(IApiClient apiClient, int page = 0)
    {
        var res = await apiClient.GetAllGroups();
        var events = res.Wrap();
        
        return KeyboardGenerator.GenerateKeyboardPage(events, "all_groups_getter", page, lineSize: 1);
    }
    
}