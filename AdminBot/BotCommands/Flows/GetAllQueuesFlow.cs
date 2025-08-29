using AdminBot.Conversations;
using AdminBot.Models;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Queue;
using AdminBot.Services.Utils;


namespace AdminBot.BotCommands.Flows;

using static Services.Controllers.DataGetterController;

public class GetAllQueuesFlow: ConversationFlow
{

    public GetAllQueuesFlow(IQueueController queueController, IApiClient apiClient)
    {
        var askNameStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var events = await GetAllQueueEvents(apiClient);
                await manager.NotificationService.SendTextMessageAsync(conversation.ChatId, "Выберите событие:",
                    replyMarkup: events);
            },
            OnCallbackQuery = async (manager, update) =>
            {
                CallbackDataView view = CallbackDataView.LoadFromCallback(update.GetCallbackText());
                
                if (view.CallbackName == "page")
                {
                    var events = await GetAllQueueEvents(apiClient, page: int.Parse(view.ExtraParam!));
                    
                    await manager.NotificationService.EditMessageReplyMarkupAsync(
                        update.GetChatId(), 
                        update.GetMessageId(),
                        replyMarkup: events);
                    return new StepResult() { State = StepResultState.Nothing, ResultingState = null };
                }
                
                await manager.NotificationService.EditMessageTextAsync(
                    update.GetChatId(), 
                    update.GetMessageId(),
                    $"Что то выбралось: {view.CallbackParam}",
                    replyMarkup: null);
                
                
                // TODO: action
                
                return new StepResult() { State = StepResultState.FinishFlow, ResultingState = null };
            }
        };
        
        Steps = new List<FlowStep> { askNameStep };
    }
    
    
    
    public override ConversationState CreateStateObject(long chatId, long userId)
    {
        return new ConversationState()
        {
            ChatId = chatId,
            UserId = userId,
        };
    }
}