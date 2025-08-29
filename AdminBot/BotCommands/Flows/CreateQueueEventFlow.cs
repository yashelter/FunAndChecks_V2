using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Models;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;

using static AdminBot.Utils.InputParser;


namespace AdminBot.BotCommands.Flows;

using static Services.Controllers.DataGetterController;

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
                    return new StepResult() { State = StepResultState.Nothing, ResultingState = null };
                }
                
                await manager.NotificationService.EditMessageReplyMarkupAsync(
                    update.GetChatId(), 
                    update.GetMessageId(),
                    replyMarkup: null);
                
                await manager.NotificationService.SendTextMessageAsync(update.GetChatId(), 
                    $"Что то выбралось: {view.CallbackParam}");

                state.SubjectId = int.Parse(view.CallbackParam);
                
                return new StepResult() { State = StepResultState.GoToNextStep, ResultingState = state };
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
                    return new StepResult() { State = StepResultState.RepeatStep, ResultingState = null };
                }
                
                state.EventTime = parsedDateTime;
                return new StepResult() { State = StepResultState.GoToNextStep, ResultingState = state };
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
                var state = manager.GetUserState<CreateQueueEventState>(update.Message!.Chat.Id);
                state.EventName = update.Message.Text;
                return Task.FromResult(new StepResult() { State = StepResultState.GoToNextStep, ResultingState = state });
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
                if (update.CallbackQuery is null) return new StepResult() { State = StepResultState.RepeatStep, ResultingState = null };
                var callbackData = update.CallbackQuery.Data;
                
                if (callbackData == "confirm_create_event")
                {
                    var state = manager.GetUserState<CreateQueueEventState>(update.CallbackQuery.From.Id);
                    
                    await manager.ApiClient.CreateQueueEvent(update.GetUserId(), state.EventName ?? string.Empty, 
                        state.EventTime, state.SubjectId);
                    
                    await manager.NotificationService.EditMessageTextAsync(
                        update.GetChatId(),
                        update.GetMessageId(),
                        text: "Событие успешно создано\n" +
                              $"<blockquote>{update.GetMessageText()}</blockquote>");
                    
                    return new StepResult() { State = StepResultState.FinishFlow, ResultingState = null };
                }
                else if (callbackData == "cancel_create_event")
                {
                    await manager.NotificationService.EditMessageTextAsync(
                        update.GetChatId(),
                        update.GetMessageId(),
                        text: "Создание события отменено\n" +
                              $"<blockquote>{update.GetMessageText()}</blockquote>");
                    
                    return new StepResult() { State = StepResultState.FinishFlow, ResultingState = null };
                }
                return new StepResult() { State = StepResultState.RepeatStep, ResultingState = null };
            }
        };
        Steps = [askSubjectIdStep, askTimeStep, askNameStep, confirmStep];
    }
    

    public override ConversationState CreateStateObject(long chatId, long userId)
    {
        return new CreateQueueEventState()
        {
            UserId = userId,
            ChatId = chatId,
        };
    }
}