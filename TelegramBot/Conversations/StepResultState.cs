namespace TelegramBot.Conversations;

public enum StepResultState
{
    GoToNextStep, 
    RepeatStep, 
    FinishFlow, 
    CancelFlow,
    Nothing,
}
