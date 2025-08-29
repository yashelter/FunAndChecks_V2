namespace AdminBot.Conversations;

public abstract class ConversationFlow
{
    public List<FlowStep> Steps { get; protected set; } = new();
    public abstract ConversationState CreateStateObject(long chatId, long userId);
}