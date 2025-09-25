using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;
using FunAndChecks.Models.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.BotCommands.Flows;

public class ParticipantActionFlow : ConversationFlow
{
    public Func<Task>? AtEnd { get; set; }

    public ParticipantActionFlow(IApiClient apiClient)
    {
        var showMenuStep = new FlowStep
        {
            OnEnter = async (manager, state) =>
            {
                var actionState = (ParticipantActionState)state;
                var text = $"Выбран участник: <b>{actionState.ParticipantFullName}</b>\n\n" +
                           "Выберите действие:";
                
                await apiClient.UpdateQueueState(
                    actionState.UserId,
                    actionState.ParticipantId, 
                    actionState.EventId,
                    QueueUserStatus.Checking);
                
                var keyboard = new InlineKeyboardMarkup([
                    [InlineKeyboardButton.WithCallbackData("📝 К выбору задачи", "action_create_submission")],
                    [InlineKeyboardButton.WithCallbackData("✅ Завершить", "action_set_status_finished")],
                    [InlineKeyboardButton.WithCallbackData("❌ Пропустить", "action_set_status_skipped")],
                    [InlineKeyboardButton.WithCallbackData("⬅️ Назад к очереди", "action_back_to_queue")]
                ]);

                await manager.NotificationService.SendTextMessageAsync(state.ChatId, text, replyMarkup: keyboard);
            },
            OnCallbackQuery = async (manager, update) =>
            {
                var actionState = manager.GetUserState<ParticipantActionState>(update.GetChatId());
                var callbackData = update.GetCallbackText();

                switch (callbackData)
                {
                    case "action_create_submission":
                        var submissionState = new CreateSubmissionState()
                        {
                            ChatId = actionState.ChatId,
                            UserId = actionState.UserId,
                            StudentId = actionState.ParticipantId,
                            SubjectId = actionState.SubjectId,

                        };
                        
                        var submissionFlow = new CreateSubmissionFlow(apiClient)
                        {
                            AtEnd = async () =>
                            {
                                manager.FinishConversation(actionState.UserId);
                                await manager.StartFlowAsync(new ParticipantActionFlow(apiClient), actionState);
                            }
                        };
                        
                        manager.FinishConversation(update.GetUserId()); // Вложенные запрещены, поэтому трюк
                        await manager.StartFlowAsync(submissionFlow, submissionState);
                        return StepResultState.Nothing;

                    case "action_set_status_finished":
                        await apiClient.UpdateQueueState(
                            actionState.UserId,
                            actionState.ParticipantId, 
                            actionState.EventId,
                            QueueUserStatus.Finished);

                        await manager.NotificationService.SendTextMessageAsync(actionState.ChatId, "Статус участника изменен на 'Завершил'.");
                        AtEnd?.Invoke();
                        return StepResultState.FinishFlow;
                    
                    case "action_set_status_skipped":
                        await apiClient.UpdateQueueState(
                            actionState.UserId,
                            actionState.ParticipantId, 
                            actionState.EventId,
                            QueueUserStatus.Skipped);
                        
                        AtEnd?.Invoke();
                        return StepResultState.FinishFlow;

                    case "action_back_to_queue":
                        await apiClient.UpdateQueueState(
                            actionState.UserId,
                            actionState.ParticipantId, 
                            actionState.EventId,
                            QueueUserStatus.Waiting);
                        AtEnd?.Invoke();
                        return StepResultState.FinishFlow;
                }
                
                return StepResultState.RepeatStep;
            }
        };
        Steps = [showMenuStep];
    }
}