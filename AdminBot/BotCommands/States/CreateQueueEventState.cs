using AdminBot.Conversations;

namespace AdminBot.BotCommands.States;

public class CreateQueueEventState: ConversationState
{
    public string? EventName { get; set; }
    public DateTime? EventTime { get; set; }
    public int SubjectId { get; set; }

}