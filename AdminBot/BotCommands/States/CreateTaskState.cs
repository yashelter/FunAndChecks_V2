using AdminBot.Conversations;

namespace AdminBot.BotCommands.States;

public class CreateTaskState: ConversationState
{
    public int SubjectId { get; set; }
    public string? Name { get; set; }
    public int MaxPoints { get; set; }

    public string Description { get; set; } = "none";  // not used yet, but have in api

}