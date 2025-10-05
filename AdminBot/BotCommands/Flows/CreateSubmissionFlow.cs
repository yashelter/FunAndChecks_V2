using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Models;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;
using FunAndChecks.Models.Enums;
using Telegram.Bot.Types.Enums;

namespace AdminBot.BotCommands.Flows;

using static Services.Controllers.DataGetterController;


public class CreateSubmissionFlow: ConversationFlow
{
    public Func<Task>? AtEnd { get; set; } // если не будет стоять всё взорвется
    
    
    public CreateSubmissionFlow(IApiClient apiClient)
    {
        var askTaskId = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var state = manager.GetUserState<CreateSubmissionState>(conversation.ChatId);
                var events = await GetAllUserTasks(state.StudentId, state.SubjectId, apiClient);
                await manager.NotificationService.SendTextMessageAsync(conversation.ChatId, "Выберите задачу:", replyMarkup: events);
            },
            OnCallbackQuery = async (manager, update) =>
            {
                var state = manager.GetUserState<CreateSubmissionState>(update.GetChatId());
                
                CallbackDataView view = CallbackDataView.LoadFromCallback(update.GetCallbackText());
                
                if (view.CallbackName == "page")
                {
                    var events = await GetAllUserTasks(
                        state.StudentId, 
                        state.SubjectId,
                        apiClient,
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

                state.TaskId = int.Parse(view.CallbackParam);
                return StepResultState.GoToNextStep;
            }
        };
        
        var askTaskState = new FlowStep()
        {
            
            OnEnter = async (manager, conversation) =>
            {
                var state = manager.GetUserState<CreateSubmissionState>(conversation.ChatId);

                var logs = await GetAllUserTaskLogs(conversation.UserId, state.StudentId, state.TaskId, apiClient);
                if (logs != null)
                {
                    foreach (var log in logs)
                    {
                        await manager.NotificationService.SendTextMessageAsync(conversation.ChatId, log, parseMode: ParseMode.Html);
                    }
                }
                await manager.NotificationService.SendConfirmationAsync(
                    conversation.ChatId,
                    text: $"Решите судьбу:\n",
                    yesCallback: "approve_task",
                    noCallback: "reject_task",
                    yesReply: "Принять ✅",
                    noReply: "ОТКЛОНИТЬ ❌"
                );
            },
            OnCallbackQuery = async (manager, update) =>
            {
                if (update.CallbackQuery is null) return StepResultState.Nothing;
                var callbackData = update.CallbackQuery.Data;
                
                if (callbackData == "approve_task")
                {
                    var state = manager.GetUserState<CreateSubmissionState>(update.GetUserId());
                    
                    await manager.NotificationService.EditMessageTextAsync(
                        update.GetChatId(),
                        update.GetMessageId(),
                        text: "Задача принята\n",
                        replyMarkup: null);
                    
                    await apiClient.CreateSubmission(update.GetUserId(), state.StudentId, state.TaskId,  SubmissionStatus.Accepted ,"Принято");
                    await manager.NotificationService.SendTextMessageAsync(update.GetChatId(), "Успешно выполнено", parseMode: ParseMode.Html);

                    AtEnd?.Invoke();
                    
                    return StepResultState.Nothing; // crutch!!
                }
                else if (callbackData == "reject_task")
                {
                    await manager.NotificationService.EditMessageTextAsync(
                        update.GetChatId(),
                        update.GetMessageId(),
                        text: "Задача отклонена\n",
                        replyMarkup: null);
                    
                    return StepResultState.GoToNextStep;
                }
                return StepResultState.Nothing;
            }
        };
        
        var askTaskComment = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                await manager.NotificationService.SendTextMessageAsync(conversation.ChatId, 
                    "Введите комментарий к задаче, если очень лень," +
                    " и не хотите помочь товарищу в будущем: /none", parseMode: ParseMode.Html);
            },
            OnResponse = async (manager, update) =>
            {
                if (update.Message is null) return StepResultState.RepeatStep;
                var message = update.GetMessageText();
                
                if (message == "/none")
                {
                    message = "Проверяющий поленился пояснить своё решение. [сгенерировано]";
                }
                var state = manager.GetUserState<CreateSubmissionState>(update.GetUserId());
                state.Comment = message;
                
                await apiClient.CreateSubmission(update.GetUserId(), state.StudentId, state.TaskId,  SubmissionStatus.Rejected ,state.Comment);
                await manager.NotificationService.SendTextMessageAsync(update.GetChatId(), "Успешно выполнено", parseMode: ParseMode.Html);

                AtEnd?.Invoke();
                
                return StepResultState.Nothing; // crutch!!
            }
        };

        Steps = [askTaskId, askTaskState, askTaskComment];
    }
    
}