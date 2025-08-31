using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Models;
using AdminBot.Services.Utils;

namespace AdminBot.BotCommands.Flows;

using static Services.Controllers.DataGetterController;


public class CreateTaskFlow: ConversationFlow
{
    public CreateTaskFlow()
    {
        var askSubjectId = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var events = await GetAllSubjects(manager.ApiClient);
                await manager.NotificationService.SendTextMessageAsync(conversation.ChatId, "Выберите предмет:", replyMarkup: events);
            },
            OnCallbackQuery = async (manager, update) =>
            {
                var state = manager.GetUserState<CreateTaskState>(update.GetChatId());
                
                CallbackDataView view = CallbackDataView.LoadFromCallback(update.GetCallbackText());
                
                if (view.CallbackName == "page")
                {
                    var events = await GetAllSubjects(manager.ApiClient,
                        page: int.Parse(view.ExtraParam!));
                    
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
        
        var askNameStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                await manager.NotificationService.SendTextMessageAsync(
                        conversation.ChatId, 
                    $"Введите название задачи:" +
                    $"<blockquote>Рекомендуется например: DES 1.1</blockquote>");
            },
            OnResponse = (manager, update) =>
            {
                var state = manager.GetUserState<CreateTaskState>(update.GetChatId());
                state.Name = update.GetMessageText();
                return Task.FromResult(new StepResult() { State = StepResultState.GoToNextStep, ResultingState = state });
            }
        };
        
        var askMaxPoints = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                await manager.NotificationService.SendTextMessageAsync(
                    conversation.ChatId, 
                    $"Введите количество баллов за задачу:");
            },
            OnResponse = async (manager, update) =>
            {
                var state = manager.GetUserState<CreateTaskState>(update.GetChatId());
                int maxPoints = int.TryParse(update.GetMessageText(), out int max) ? max : 0;

                if (maxPoints <= 0)
                {
                    await manager.NotificationService.SendTextMessageAsync(
                        update.GetChatId(), 
                        $"Введите количество баллов за задачу (>0!!!):");
                }
                
                state.MaxPoints = maxPoints;
                
                return new StepResult() { State = StepResultState.GoToNextStep, ResultingState = state };
            }
        };
        var confirmStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var state = manager.GetUserState<CreateTaskState>(conversation.UserId);
                var subj = await manager.ApiClient.GetSubject(state.SubjectId);
                
                string displayName = subj?.Name switch
                {
                    null or "" => "Неверное id (отмените создание)",
                    { } name => name
                };
                
                await manager.NotificationService.SendConfirmationAsync(
                    conversation.ChatId,
                    $"Вы уверены, что хотите создать задачу:\n" +
                    $"Имя: <code>{state.Name}</code>\n" +
                    $"Количество баллов: <code>{state.MaxPoints.ToString()}</code>\n" +
                    $"Предмет: <code>{displayName}</code>\n",
                    yesCallback: "confirm_create_event",
                    noCallback: "cancel_create_event"
                );
            },
            
            OnCallbackQuery = async (manager, update) =>
            {
                if (update.CallbackQuery is null) return new StepResult() { State = StepResultState.RepeatStep, ResultingState = null };
                var callbackData = update.CallbackQuery.Data;
                
                if (callbackData == "confirm_create_task")
                {
                    var state = manager.GetUserState<CreateTaskState>(update.CallbackQuery.From.Id);
                    
                    await manager.ApiClient.CreateNewTask(
                        update.GetUserId(), 
                        state.SubjectId, 
                        state.Name ?? throw new InvalidOperationException("Некое поле null"), 
                        state.MaxPoints,
                        state.Description);
                    
                    await manager.NotificationService.EditMessageTextAsync(
                        update.GetChatId(),
                        update.GetMessageId(),
                        text: "Событие успешно создано\n" +
                              $"<blockquote>{update.GetMessageText()}</blockquote>");
                    
                    return new StepResult() { State = StepResultState.FinishFlow, ResultingState = null };
                }
                else if (callbackData == "cancel_create_task")
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

        Steps = [askSubjectId, askNameStep, askMaxPoints, confirmStep];
    }
    
    public override ConversationState CreateStateObject(long chatId, long userId)
    {
        return new CreateTaskState()
        {
            ChatId = chatId,
            UserId = userId,
        };
    }
}