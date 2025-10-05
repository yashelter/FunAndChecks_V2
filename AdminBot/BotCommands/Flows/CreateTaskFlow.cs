using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Models;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;

namespace AdminBot.BotCommands.Flows;

using static Services.Controllers.DataGetterController;


public class CreateTaskFlow: ConversationFlow
{
    public CreateTaskFlow(IApiClient apiClient)
    {
        var askSubjectId = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var events = await GetAllSubjects(apiClient);
                await manager.NotificationService.SendTextMessageAsync(conversation.ChatId, "Выберите предмет:", replyMarkup: events);
            },
            OnCallbackQuery = async (manager, update) =>
            {
                var state = manager.GetUserState<CreateTaskState>(update.GetChatId());
                
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
                
                state.SubjectId = int.Parse(view.CallbackParam);
                
                return StepResultState.GoToNextStep;
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
                return Task.FromResult(StepResultState.GoToNextStep);
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
                
                return StepResultState.GoToNextStep;
            }
        };
        var confirmStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var state = manager.GetUserState<CreateTaskState>(conversation.UserId);
                var subj = await apiClient.GetSubject(state.SubjectId);
                
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
                    yesCallback: "confirm_create_task",
                    noCallback: "cancel_create_task"
                );
            },
            
            OnCallbackQuery = async (manager, update) =>
            {
                if (update.CallbackQuery is null) return StepResultState.RepeatStep;
                var callbackData = update.CallbackQuery.Data;
                
                if (callbackData == "confirm_create_task")
                {
                    var state = manager.GetUserState<CreateTaskState>(update.GetUserId());
                    
                    await apiClient.CreateNewTask(
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
                    
                    return StepResultState.FinishFlow;
                }
                else if (callbackData == "cancel_create_task")
                {
                    await manager.NotificationService.EditMessageTextAsync(
                        update.GetChatId(),
                        update.GetMessageId(),
                        text: "Создание события отменено\n" +
                              $"<blockquote>{update.GetMessageText()}</blockquote>");
                    
                    return StepResultState.FinishFlow;
                }
                return StepResultState.RepeatStep;
            }
        };

        Steps = [askSubjectId, askNameStep, askMaxPoints, confirmStep];
    }
}