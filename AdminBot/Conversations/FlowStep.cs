using Telegram.Bot.Types;

namespace AdminBot.Conversations;

public class FlowStep
{
    // Вызывается при входе на шаг
    public Func<IConversationManager, ConversationState, Task>? OnEnter { get; set; }
    
    // Вызывается, если пришел текстовый ответ
    public Func<IConversationManager, Update, Task<StepResult>>? OnResponse { get; set; }
    
    // Вызывается, если пришел ответ с кнопки
    public Func<IConversationManager, Update, Task<StepResult>>? OnCallbackQuery { get; set; }
}