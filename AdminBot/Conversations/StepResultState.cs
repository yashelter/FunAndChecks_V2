namespace AdminBot.Conversations;

public enum StepResultState
{
    GoToNextStep, 
    RepeatStep, 
    FinishFlow, 
    CancelFlow,
    Nothing,
}
