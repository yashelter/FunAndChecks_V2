using AdminBot.BotCommands.Queue;
using AdminBot.Conversations;
using AdminBot.Models;
using AdminBot.Services.ApiClient;
using AdminBot.Services.QueueManager;
using AdminBot.Services.Utils;


namespace AdminBot.BotCommands.Flows;

using static Services.Controllers.DataGetterController;

public class GetAllQueuesFlow: ConversationFlow
{

    public GetAllQueuesFlow(IQueueManager queueManager, IQueueController queueController)
    {
        var askNameStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var events = await GetAllQueueEvents(manager.ApiClient);
                await manager.NotificationService.SendTextMessageAsync(conversation.ChatId, "Выберите событие:",
                    replyMarkup: events);
            },
            OnCallbackQuery = async (manager, update) =>
            {
                CallbackDataView view = CallbackDataView.LoadFromCallback(update.GetCallbackText());
                
                if (view.CallbackName == "page")
                {
                    var events = await GetAllQueueEvents(manager.ApiClient, page: int.Parse(view.ExtraParam!));
                    
                    await manager.NotificationService.EditMessageReplyMarkupAsync(
                        update.GetChatId(), 
                        update.GetMessageId(),
                        replyMarkup: events);
                    return new StepResult() { State = StepResultState.Nothing, ResultingState = null };
                }

                int queueId = int.Parse(view.CallbackParam);

                var subs = await queueController.SubscribeToQueueEvent(update.GetUserId(), queueId);
                var res = await queueManager.SubscribeUserToQueue(subs);
                
                await manager.NotificationService.EditMessageTextAsync(
                    update.GetChatId(), 
                    update.GetMessageId(),
                    $"Выбрана очередь: {res.EventName}",
                    replyMarkup: null);
                
                return new StepResult() { State = StepResultState.FinishFlow, ResultingState = null };
            }
        };
        
        Steps = [askNameStep];
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