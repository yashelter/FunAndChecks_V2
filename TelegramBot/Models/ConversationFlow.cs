using TelegramBot.Conversations;

namespace TelegramBot.Models;

public abstract class ConversationFlow
{
    public List<FlowStep> Steps { get; protected set; } = new();
}