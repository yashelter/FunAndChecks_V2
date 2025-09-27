using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;

namespace AdminBot.BotCommands.Flows;

public class CreateSubjectFlow : ConversationFlow
{
    public CreateSubjectFlow(IApiClient apiClient)
    {
        var askNameStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                await manager.NotificationService.SendTextMessageAsync(conversation.ChatId, "Введите название предмета:");
            },
            OnResponse = (manager, update) =>
            {
                var state = manager.GetUserState<CreateSubjectState>(update.GetUserId());
                state.SubjectName = update.GetMessageText();
                return Task.FromResult(StepResultState.GoToNextStep);
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
                if (update.CallbackQuery is null) return StepResultState.RepeatStep;
                var callbackData = update.GetCallbackText();
                
                if (callbackData == "confirm_create_subject")
                {
                    var state = manager.GetUserState<CreateSubjectState>(update.GetUserId());
                    await apiClient.CreateNewSubject(update.GetUserId(), state.SubjectName!);

                    await manager.NotificationService.EditMessageTextAsync(
                            update.GetChatId(),
                            update.GetMessageId(), 
                        $"Предмет успешно создан\n" +
                        $"<blockquote>{update.GetMessageText()}</blockquote>");
                    
                    return StepResultState.FinishFlow;
                }
                else if (callbackData == "cancel_create_subject")
                {
                    await manager.NotificationService.EditMessageTextAsync
                    (update.GetChatId(),
                        update.GetMessageId(),
                        $"Случилась отмена\n" +
                        $"<blockquote>{update.GetMessageText()}</blockquote>");
                    
                    return StepResultState.FinishFlow;
                }
                return StepResultState.Nothing;
            }
        };
        Steps = [askNameStep, confirmStep];
    }
}