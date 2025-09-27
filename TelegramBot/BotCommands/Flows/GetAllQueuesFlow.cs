using TelegramBot.BotCommands.Queue;
using TelegramBot.Conversations;
using TelegramBot.Models;
using TelegramBot.Services.ApiClient;
using TelegramBot.Services.Controllers;
using TelegramBot.Utils;

namespace TelegramBot.BotCommands.Flows;

using static DataGetterController;

public class GetAllQueuesFlow: ConversationFlow
{

    public GetAllQueuesFlow(IApiClient apiClient, IQueueController queueController)
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
        
        Steps = [askNameStep];
    }
}