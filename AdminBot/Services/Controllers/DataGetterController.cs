using AdminBot.Services.ApiClient;
using AdminBot.Services.Keyboard;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.Services.Controllers;

public static class DataGetterController
{
    public static async Task<InlineKeyboardMarkup> GetAllSubjects(IApiClient apiClient, int page = 0)
    {
        var subjects = await apiClient.GetAllSubjects();
        if (subjects is null) throw new InvalidDataException("No subjects found");
        
        var res = subjects.WrapSubjects();
        
        return KeyboardGenerator.GenerateKeyboardPage(res, "subject_picker", page, lineSize: 1);
    }
    
    
    public static async Task<InlineKeyboardMarkup> GetAllQueueEvents(IApiClient apiClient, int page = 0)
    {
        var res = await apiClient.GetAllQueueEvents();
        var events = res.WrapQueueEvents();
        
        return KeyboardGenerator.GenerateKeyboardPage(events, "all_queues_getter", page, lineSize: 1);
    }
    
}