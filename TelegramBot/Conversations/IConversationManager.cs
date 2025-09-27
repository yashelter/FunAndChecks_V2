using Telegram.Bot.Types;
using TelegramBot.Models;

namespace TelegramBot.Conversations;

public interface IConversationManager
{
    Task StartFlowAsync(ConversationFlow flow, ConversationState initialState);
    
    Task ProcessResponseAsync(Update update);
    
    Task<bool> IsUserInConversationAsync(long userId);
    
    T GetUserState<T>(long userId) where T : ConversationState;
    
    INotificationService NotificationService { get; }
    
    Task ResetUserState(long userId);

    void FinishConversation(long userId);
}
