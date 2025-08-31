using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Models;
using AdminBot.Services.Utils;

namespace AdminBot.BotCommands.Flows;

using static Services.Controllers.DataGetterController;

public class LinkGroupToSubjectFlow: ConversationFlow
{
    public LinkGroupToSubjectFlow()
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
                var state = manager.GetUserState<LinkGroupToSubjectState>(update.GetChatId());
                
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
        
        var askGroupId = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var events = await GetAllGroups(manager.ApiClient);
                await manager.NotificationService.SendTextMessageAsync(conversation.ChatId, "Выберите группу:", replyMarkup: events);
            },
            OnCallbackQuery = async (manager, update) =>
            {
                var state = manager.GetUserState<LinkGroupToSubjectState>(update.GetChatId());
                
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

                state.GroupId = int.Parse(view.CallbackParam);
                
                return new StepResult() { State = StepResultState.GoToNextStep, ResultingState = state };
            }
        };
        
        var confirmStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var state = manager.GetUserState<LinkGroupToSubjectState>(conversation.UserId);
                var subj = await manager.ApiClient.GetSubject(state.SubjectId);
                var group = await manager.ApiClient.GetGroup(state.GroupId);
                
                string subjName = subj?.Name switch
                {
                    null or "" => "Неверное id (отмените создание)",
                    { } name => name
                };
                
                string subjGroup = group?.Name switch
                {
                    null or "" => "Неверное id (отмените создание)",
                    { } name => name
                };
                
                await manager.NotificationService.SendConfirmationAsync(
                    conversation.ChatId,
                    $"Вы уверены, что хотите дать доступ группе" +
                    $": <code>{subjGroup}</code>\n" +
                    $"К предмету: <code>{subjName}</code>\n",
                    yesCallback: "confirm_create_link_gt",
                    noCallback: "cancel_create_link_gt"
                );
            },
            
            OnCallbackQuery = async (manager, update) =>
            {
                if (update.CallbackQuery is null) return new StepResult() { State = StepResultState.RepeatStep, ResultingState = null };
                var callbackData = update.CallbackQuery.Data;
                
                if (callbackData == "confirm_create_link_gt")
                {
                    var state = manager.GetUserState<LinkGroupToSubjectState>(update.GetUserId());

                    await manager.ApiClient.LinkGroupToSubject(update.GetUserId(), groupId: state.GroupId,
                        subjectId: state.SubjectId);
                    
                    await manager.NotificationService.EditMessageTextAsync(
                        update.GetChatId(),
                        update.GetMessageId(),
                        text: "Связь успешно создана\n" +
                              $"<blockquote>{update.GetMessageText()}</blockquote>");
                    
                    return new StepResult() { State = StepResultState.FinishFlow, ResultingState = null };
                }
                else if (callbackData == "cancel_create_link_gt")
                {
                    await manager.NotificationService.EditMessageTextAsync(
                        update.GetChatId(),
                        update.GetMessageId(),
                        text: "Создание связи отменено\n" +
                              $"<blockquote>{update.GetMessageText()}</blockquote>");
                    
                    return new StepResult() { State = StepResultState.FinishFlow, ResultingState = null };
                }
                return new StepResult() { State = StepResultState.RepeatStep, ResultingState = null };
            }
        };

        Steps = [askSubjectId, askGroupId, confirmStep];
    }
    
    
    public override ConversationState CreateStateObject(long chatId, long userId)
    {
        return new LinkGroupToSubjectState()
        {
            ChatId = chatId,
            UserId = userId,
        };
    }
}