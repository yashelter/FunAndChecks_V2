using AdminBot.Conversations;

namespace AdminBot.BotCommands.States;

public class CreateGroupState: ConversationState
{
    public string? GroupName { get; set; }
    public int GroupNumber { get; set;}
    public int StartYear { get; set;}
}