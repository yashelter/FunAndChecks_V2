namespace AdminBot.Conversations;

public enum StepResultState
{
    GoToNextStep, 
    RepeatStep, 
    FinishFlow, 
    CancelFlow,
    Nothing,
}

public class StepResult
{
    public required StepResultState State {get; set;}
    public ConversationState? ResultingState {get; set;}
}

