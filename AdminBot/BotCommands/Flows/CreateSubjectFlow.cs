using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Services.Utils;

namespace AdminBot.BotCommands.Flows;

public class CreateSubjectFlow : ConversationFlow
{
    public CreateSubjectFlow()
    {
        var askNameStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                await manager.NotificationService.SendTextMessageAsync(conversation.ChatId, "Введите название предмета:");
            },
            OnResponse = (manager, update) =>
            {
                var state = manager.GetUserState<CreateSubjectState>(update.GetChatId());
                state.SubjectName = update.GetMessageText();
                return Task.FromResult(new StepResult() { State = StepResultState.GoToNextStep, ResultingState = state });
            }
        };
        
        var confirmStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var state = manager.GetUserState<CreateSubjectState>(conversation.UserId);
                
                await manager.NotificationService.SendConfirmationAsync(
                    conversation.ChatId,
                    $"Вы уверены, что хотите создать предмет:\n" +
                    $"<code>{state.SubjectName}</code>",
                    yesCallback: "confirm_create_subject",
                    noCallback: "cancel_create_subject"
                );
            },
            
            OnCallbackQuery = async (manager, update) =>
            {
                if (update.CallbackQuery is null) return new StepResult() { State = StepResultState.RepeatStep, ResultingState = null };
                var callbackData = update.GetCallbackText();
                
                if (callbackData == "confirm_create_subject")
                {
                    var state = manager.GetUserState<CreateSubjectState>(update.GetUserId());
                    await manager.ApiClient.CreateNewSubject(update.GetUserId(), state.SubjectName!);

                    await manager.NotificationService.EditMessageTextAsync(
                            update.GetChatId(),
                            update.GetMessageId(), 
                        $"Предмет успешно создан\n" +
                        $"<blockquote>{update.GetMessageText()}</blockquote>");
                    
                    return new StepResult() { State = StepResultState.FinishFlow, ResultingState = null };
                }
                else if (callbackData == "cancel_create_subject")
                {
                    await manager.NotificationService.EditMessageTextAsync
                    (update.GetChatId(),
                        update.GetMessageId(),
                        $"Случилась отмена\n" +
                        $"<blockquote>{update.GetMessageText()}</blockquote>");
                    
                    return new StepResult() { State = StepResultState.FinishFlow, ResultingState = null };
                }
                return new StepResult() { State = StepResultState.Nothing, ResultingState = null };
            }
        };
        Steps = [askNameStep, confirmStep];
    }

    public override ConversationState CreateStateObject(long chatId, long userId)
    {
        return new CreateSubjectState()
        {
            ChatId = chatId,
            UserId = userId,
        };
    }
}