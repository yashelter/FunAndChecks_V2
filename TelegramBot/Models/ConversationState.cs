namespace TelegramBot.Models;

public class ConversationState
{
    public required long ChatId { get; set; }
    public required long UserId { get; set; }
}