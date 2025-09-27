using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;

using static AdminBot.Utils.InputParser;

namespace AdminBot.BotCommands.Flows;


public class CreateGroupFlow: ConversationFlow
{
    public CreateGroupFlow(IApiClient apiClient)
    {
        var askNameStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                await manager.NotificationService.SendTextMessageAsync(conversation.ChatId, "Введите название группы:");
            },
            
            OnResponse = async (manager, update) =>
            {
                var state = manager.GetUserState<CreateGroupState>(update.GetUserId());
                
                state.GroupName = update.GetMessageText();
                var parsed = ParseGroupString(state.GroupName);
                
                if (parsed == null)
                {
                    await manager.NotificationService.SendTextMessageAsync(
                        update.GetChatId(), 
                        "Введено не очень. Регулярка не смогла. Введите название группы ещё разик:");
                    return StepResultState.Nothing;
                }

                state.GroupNumber = parsed.Value.YY;
                state.StartYear = parsed.Value.ZZ;
                
                return StepResultState.GoToNextStep;
            }
        };
        
        var confirmStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var state = manager.GetUserState<CreateGroupState>(conversation.UserId);
                
                await manager.NotificationService.SendConfirmationAsync(
                    conversation.ChatId,
                    $"Вы уверены, что хотите создать группу:\n" +
                    $"Название: <code>{state.GroupName}</code>\n" +
                    $"Номер группы (без курса): <code>{state.GroupNumber}</code>\n" +
                    $"Год начала обучений<code>{state.StartYear}</code>\n",
                    yesCallback: "confirm_create_group",
                    noCallback: "cancel_create_group"
                );
            },
            
            OnCallbackQuery = async (manager, update) =>
            {
                if (update.CallbackQuery is null) return StepResultState.RepeatStep;
                var callbackData = update.GetCallbackText();
                
                if (callbackData == "confirm_create_group")
                {
                    var state = manager.GetUserState<CreateGroupState>(update.GetUserId());
                    
                    await apiClient.CreateNewGroup(
                        update.GetUserId(), 
                        state.GroupName!,
                        state.GroupNumber,
                        state.StartYear);

                    await manager.NotificationService.EditMessageTextAsync(
                            update.GetChatId(),
                            update.GetMessageId(), 
                        $"Действие успешно выполнено\n" +
                        $"<blockquote>{update.GetMessageText()}</blockquote>");
                    
                    return StepResultState.FinishFlow;
                }
                else if (callbackData == "cancel_create_group")
                {
                    await manager.NotificationService.SendTextMessageAsync(
                        update.GetChatId(),
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