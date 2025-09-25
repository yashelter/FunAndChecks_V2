using AdminBot.Conversations;

namespace AdminBot.BotCommands.States;

public class ParticipantActionState : ConversationState
{
    public int EventId { get; set; }
    public string ParticipantId { get; set; }
    public string ParticipantFullName { get; set; } 
    public int SubjectId { get; set; }
}