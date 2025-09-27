using Telegram.Bot.Types;
using TelegramBot.Models;

namespace TelegramBot.Conversations;

public class FlowStep
{
    // Вызывается при входе на шаг
    public Func<IConversationManager, ConversationState, Task>? OnEnter { get; set; }
    
    // Вызывается, если пришел текстовый ответ
    public Func<IConversationManager, Update, Task<StepResultState>>? OnResponse { get; set; }
    
    // Вызывается, если пришел ответ с кнопки
    public Func<IConversationManager, Update, Task<StepResultState>>? OnCallbackQuery { get; set; }
}