using AdminBot.Services.ApiClient;
using Telegram.Bot.Types;

namespace AdminBot.Conversations;

public interface IConversationManager
{
    Task StartFlowAsync(ConversationFlow flow, long chatId, long userId);
    
    Task ProcessResponseAsync(Update update);
    
    Task<bool> IsUserInConversationAsync(long userId);
    T GetUserState<T>(long userId) where T : ConversationState, new();
    
    INotificationService NotificationService { get; }
    IApiClient ApiClient { get; }
}
