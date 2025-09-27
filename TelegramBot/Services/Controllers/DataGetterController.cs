using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Services.ApiClient;
using TelegramBot.Services.Keyboard;

namespace TelegramBot.Services.Controllers;

public static class DataGetterController
{
    public static async Task<InlineKeyboardMarkup> GetAllUserTasks(string userId, int subjectId, IApiClient apiClient, int page = 0)
    {
        var tasks = await apiClient.GetAllUserTasks(userId, subjectId);
        if (tasks is null) throw new InvalidDataException("No subjects found");
        
        var res = tasks.Wrap();
        return KeyboardGenerator.GenerateKeyboardPage(res, $"tasks_picker[{subjectId}]", page, lineSize: 1);
    }
    
    // TODO: Возможно это надо, но пока что api не позволяет студентам смотреть логи
    /*public static async Task<List<string>?> GetAllUserTaskLogs(long adminId,
        string userId,
        int taskId,
        IApiClient apiClient,
        int page = 0)
    {
        var tasks = await apiClient.GetAllTaskLogs(adminId, userId, taskId);
        var res = tasks.Wrap();
        return res;
    }*/
    
    
    public static async Task<InlineKeyboardMarkup> GetAllSubjects(IApiClient apiClient, int page = 0)
    {
        var subjects = await apiClient.GetAllSubjects();
        if (subjects is null) throw new InvalidDataException("No subjects found");
        
        var res = subjects.Wrap();
        
        return KeyboardGenerator.GenerateKeyboardPage(res, "subject_picker", page, lineSize: 1);
    }
    
    public static async Task<InlineKeyboardMarkup> GetAllMySubjects(long telegramId, IApiClient apiClient, int page = 0)
    {
        var subjects = await apiClient.GetMySubjects(telegramId);
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
    
    public static async Task<InlineKeyboardMarkup> GetMyQueueEvents(long telegramId, IApiClient apiClient, int page = 0)
    {
        var res = await apiClient.GetMyQueueEvents(telegramId);
        var events = res.Wrap();
        
        return KeyboardGenerator.GenerateKeyboardPage(events, "my_queues_getter", page, lineSize: 1);
    }
    
    public static async Task<InlineKeyboardMarkup> GetAllGroups(IApiClient apiClient, int page = 0)
    {
        var res = await apiClient.GetAllGroups();
        var events = res.Wrap();
        
        return KeyboardGenerator.GenerateKeyboardPage(events, "all_groups_getter", page, lineSize: 1);
    }
    
}