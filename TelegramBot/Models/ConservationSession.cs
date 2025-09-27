namespace TelegramBot.Models;

public class ActiveConversationSession
{
    public required long UserId { get; set; }
    
    public required long ChatId { get; set; }
    
    
    public required ConversationFlow Flow { get; set; }
    
    public required ConversationState State { get; set; }

    public int CurrentStepIndex { get; set; } = 0;
}