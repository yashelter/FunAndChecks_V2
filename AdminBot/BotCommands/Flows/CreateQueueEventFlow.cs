using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Models;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;

using static AdminBot.Utils.InputParser;
using static AdminBot.Services.Controllers.DataGetterController;


namespace AdminBot.BotCommands.Flows;


public class CreateQueueEventFlow: ConversationFlow
{
    public CreateQueueEventFlow(IApiClient apiClient)
    {
        
        var askSubjectIdStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var events = await GetAllSubjects(apiClient);
                await manager.NotificationService.SendTextMessageAsync(conversation.ChatId, "Выберите предмет:", replyMarkup: events);
            },
            OnCallbackQuery = async (manager, update) =>
            {
                var state = manager.GetUserState<CreateQueueEventState>(update.GetChatId());
                CallbackDataView view = CallbackDataView.LoadFromCallback(update.GetCallbackText());
                
                if (view.CallbackName == "page")
                {
                    var events = await GetAllSubjects(apiClient, page: int.Parse(view.ExtraParam!));
                    
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
        
        
        var askTimeStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                await manager.NotificationService.SendTextMessageAsync(conversation.ChatId,
                    "Введите время события (dd.MM.yyyy HH:mm):");
            },
            OnResponse = async (manager, update) =>
            {
                var state = manager.GetUserState<CreateQueueEventState>(update.GetChatId());
                DateTime? parsedDateTime = ParseDateTimeString(update.GetMessageText());

                if (parsedDateTime == null)
                {
                    await manager.NotificationService.SendTextMessageAsync(update.GetChatId(), 
                        $"Нужно следовать формату, повторите ввод");
                    return StepResultState.RepeatStep;
                }
                
                state.EventTime = parsedDateTime;
                return StepResultState.GoToNextStep;
            }
        };

        
        var askNameStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                await manager.NotificationService.SendTextMessageAsync(conversation.ChatId, "Введите название события:");
            },
            OnResponse = (manager, update) =>
            {
                var state = manager.GetUserState<CreateQueueEventState>(update.GetUserId());
                state.EventName = update.GetMessageText();
                return Task.FromResult(StepResultState.GoToNextStep);
            }
        };
        
        
        var confirmStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var state = manager.GetUserState<CreateQueueEventState>(conversation.UserId);
                var subj = await apiClient.GetSubject(state.SubjectId);
                
                string displayName = subj?.Name switch
                {
                    null or "" => "Неверное id (отмените создание)",
                    { } name => name
                };
                
                await manager.NotificationService.SendConfirmationAsync(
                    conversation.ChatId,
                    $"Вы уверены, что хотите создать событие:\n" +
                    $"Имя: <code>{state.EventName}</code>\n" +
                    $"Время проведения: <code>{state.EventTime.ToString()}</code>\n" +
                    $"Предмет: <code>{displayName}</code>\n",
                    yesCallback: "confirm_create_event",
                    noCallback: "cancel_create_event"
                );
            },
            
            OnCallbackQuery = async (manager, update) =>
            {
                if (update.CallbackQuery is null) return StepResultState.RepeatStep;
                
                var callbackData = update.CallbackQuery.Data;
                
                switch (callbackData)
                {
                    case "confirm_create_event":
                    {
                        var state = manager.GetUserState<CreateQueueEventState>(update.CallbackQuery.From.Id);
                    
                        await apiClient.CreateQueueEvent(update.GetUserId(), state.EventName ?? string.Empty, 
                            state.EventTime, state.SubjectId);
                    
                        await manager.NotificationService.EditMessageTextAsync(
                            update.GetChatId(),
                            update.GetMessageId(),
                            text: "Событие успешно создано\n" +
                                  $"<blockquote>{update.GetMessageText()}</blockquote>");
                    
                        return StepResultState.FinishFlow;
                    }
                    case "cancel_create_event":
                        await manager.NotificationService.EditMessageTextAsync(
                            update.GetChatId(),
                            update.GetMessageId(),
                            text: "Создание события отменено\n" +
                                  $"<blockquote>{update.GetMessageText()}</blockquote>");
                    
                        return StepResultState.FinishFlow;
                    default:
                        return StepResultState.RepeatStep;
                }
            }
        };
        Steps = [askSubjectIdStep, askTimeStep, askNameStep, confirmStep];
    }
}