using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Services.Utils;

using static AdminBot.Utils.InputParser;
namespace AdminBot.BotCommands.Flows;

public class CreateGroupFlow: ConversationFlow
{
    public CreateGroupFlow()
    {
        var askNameStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                await manager.NotificationService.SendTextMessageAsync(conversation.ChatId, "Введите название группы:");
            },
            OnResponse = async (manager, update) =>
            {
                var state = manager.GetUserState<CreateGroupState>(update.GetChatId());
                state.GroupName = update.GetMessageText();
                var parsed = ParseGroupString(state.GroupName);
                
                if (parsed == null)
                {
                    await manager.NotificationService.SendTextMessageAsync(update.GetChatId(), "Введено не очень. Регулярка не смогла. " +
                        "Введите название группы ещё разик:");
                    return new StepResult() { State = StepResultState.Nothing, ResultingState = null };
                }

                state.GroupNumber = parsed.Value.YY;
                state.StartYear = parsed.Value.ZZ;
                
                return new StepResult() { State = StepResultState.GoToNextStep, ResultingState = state };
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
                if (update.CallbackQuery is null) return new StepResult() { State = StepResultState.RepeatStep, ResultingState = null };
                var callbackData = update.GetCallbackText();
                
                if (callbackData == "confirm_create_group")
                {
                    var state = manager.GetUserState<CreateGroupState>(update.GetUserId());
                    await manager.ApiClient.CreateNewGroup(update.GetUserId(), state.GroupName!, state.GroupNumber, state.StartYear);

                    await manager.NotificationService.EditMessageTextAsync(
                            update.GetChatId(),
                            update.GetMessageId(), 
                        $"Действие успешно выполнено\n" +
                        $"<blockquote>{update.GetMessageText()}</blockquote>");
                    
                    return new StepResult() { State = StepResultState.FinishFlow, ResultingState = null };
                }
                else if (callbackData == "cancel_create_group")
                {
                    await manager.NotificationService.SendTextMessageAsync(update.CallbackQuery.From.Id,
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
        return new CreateGroupState()
        {
            ChatId = chatId,
            UserId = userId,
        };
    }
}