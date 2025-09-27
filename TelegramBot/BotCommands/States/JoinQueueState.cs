using TelegramBot.Models;

namespace TelegramBot.BotCommands.States;

public class JoinQueueState : ConversationState
{
    public int SubjectId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; }
    public string SubjectName { get; set; }
}