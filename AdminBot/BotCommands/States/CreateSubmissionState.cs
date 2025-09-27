using AdminBot.Conversations;
using FunAndChecks.Models.Enums;

namespace AdminBot.BotCommands.States;

public class CreateSubmissionState: ConversationState
{
    public string StudentId { get; set; }
    public int SubjectId { get; set; }
    public int TaskId { get; set; }
    public string Comment {get; set;}
    public SubmissionStatus Status { get; set; }
}