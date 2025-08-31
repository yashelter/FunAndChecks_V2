using AdminBot.Conversations;

namespace AdminBot.BotCommands.States;


public class LinkGroupToSubjectState: ConversationState
{
    public int GroupId { get; set; }
    public int SubjectId { get; set; }
}