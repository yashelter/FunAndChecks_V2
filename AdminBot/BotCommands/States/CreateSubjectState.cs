using AdminBot.Conversations;

namespace AdminBot.BotCommands.States;

public class CreateSubjectState : ConversationState
{
    public string? SubjectName { get; set; }
}