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

    public GetAllQueuesFlow(IApiClient apiClient, IQueueController queueController)
    {
        var askEventStep = new FlowStep()
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
                    return StepResultState.Nothing;
                }

                int queueId = int.Parse(view.CallbackParam);

                var res = await queueController.SubscribeToQueueEvent(update.GetUserId(), queueId);
                
                await manager.NotificationService.EditMessageTextAsync(
                    update.GetChatId(), 
                    update.GetMessageId(),
                    $"Выбрана очередь: {res.EventName}",
                    replyMarkup: null);
                
                return StepResultState.FinishFlow;
            }
        };
        
        Steps = [askEventStep];
    }
}