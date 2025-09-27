using TelegramBot.BotCommands.States;
using TelegramBot.Conversations;
using TelegramBot.Models;
using TelegramBot.Services.ApiClient;
using TelegramBot.Services.Controllers;
using TelegramBot.Utils;

using static TelegramBot.Services.Controllers.DataGetterController;

namespace TelegramBot.BotCommands.Flows;

public class JoinQueueFlow : ConversationFlow
{
    public JoinQueueFlow(IApiClient apiClient)
    {
        var askSubjectIdStep = new FlowStep()
        {
            OnEnter = async (manager, state) =>
            {
                try
                {
                    var subjects = await GetAllMySubjects(state.UserId, apiClient);
                    await manager.NotificationService.SendTextMessageAsync(state.ChatId, "Выберите предмет:", replyMarkup: subjects);
                }
                catch (Exception e)
                {
                    await manager.NotificationService.SendTextMessageAsync(state.ChatId, "Для вашей группы не назначено ни одного предмета.");
                    manager.FinishConversation(state.ChatId);
                    return;
                }
            },
            OnCallbackQuery = async (manager, update) =>
            {
                var state = manager.GetUserState<JoinQueueState>(update.GetChatId());
                
                CallbackDataView view = CallbackDataView.LoadFromCallback(update.GetCallbackText());
                
                if (view.CallbackName == "page")
                {
                    var events = await GetAllSubjects(apiClient,
                        page: int.Parse(view.ExtraParam!));
                    
                    await manager.NotificationService.EditMessageReplyMarkupAsync(
                        update.GetChatId(), 
                        update.GetMessageId(),
                        replyMarkup: events);
                    return StepResultState.Nothing;
                }
                
                await manager.NotificationService.EditMessageReplyMarkupAsync(
                    update.GetChatId(), 
                    update.GetMessageId(),
                    replyMarkup: null);
                
                await manager.NotificationService.SendTextMessageAsync(update.GetChatId(), 
                    $"Что то выбралось: {view.CallbackParam}");

                state.SubjectId = int.Parse(view.CallbackParam);
                
                return StepResultState.GoToNextStep;
            }
        };

        var askEventStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var events = await GetMyQueueEvents(conversation.UserId, apiClient);
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
                
                var joinState = manager.GetUserState<JoinQueueState>(update.GetChatId());
                joinState.EventId = queueId;
                
                await apiClient.JoinQueue(update.GetUserId(), queueId);
                await manager.NotificationService.SendTextMessageAsync(update.GetUserId(), "Запись в очередь создана.");

                return StepResultState.FinishFlow;
            }
        };
        
        Steps = [askSubjectIdStep, askEventStep];
    }
}