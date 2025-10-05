using FunAndChecks.DTO;
using TelegramBot.BotCommands.States;
using TelegramBot.Conversations;
using TelegramBot.Models;
using TelegramBot.Services.ApiClient;
using TelegramBot.Services.Controllers;
using TelegramBot.Services.Keyboard;
using TelegramBot.Utils;

namespace TelegramBot.BotCommands.Flows;
using static DataGetterController;

public class RegisterFlow : ConversationFlow
{
    public RegisterFlow(IApiClient apiClient)
    {
        var askFirstNameStep = new FlowStep
        {
            OnEnter = async (manager, state) =>
            {
                await manager.NotificationService.SendTextMessageAsync(state.ChatId,
                    "Добро пожаловать в регистрацию! \n\nВведите ваше имя:");
            },
            OnResponse = async (manager, update) =>
            {
                var registerState = manager.GetUserState<RegisterUserState>(update.GetChatId());
                registerState.FirstName = update.GetMessageText();
                registerState.TelegramUsername = update.GetUsername(); 

                return StepResultState.GoToNextStep;
            }
        };

        var askLastNameStep = new FlowStep
        {
            OnEnter = async (manager, state) =>
            {
                await manager.NotificationService.SendTextMessageAsync(state.ChatId,
                    "Отлично! Теперь введите вашу фамилию:");
            },
            OnResponse = async (manager, update) =>
            {
                var registerState = manager.GetUserState<RegisterUserState>(update.GetChatId());
                registerState.LastName = update.GetMessageText();
                return StepResultState.GoToNextStep;
            }
        };

        var askEmailStep = new FlowStep
        {
            OnEnter = async (manager, state) =>
            {
                await manager.NotificationService.SendTextMessageAsync(state.ChatId,
                    "Введите ваш email:");
            },
            OnResponse = async (manager, update) =>
            {
                var registerState = manager.GetUserState<RegisterUserState>(update.GetChatId());
                registerState.Email = update.GetMessageText();
                return StepResultState.GoToNextStep;
            }
        };

        var askPasswordStep = new FlowStep
        {
            OnEnter = async (manager, state) =>
            {
                await manager.NotificationService.SendTextMessageAsync(state.ChatId,
                    "Придумайте пароль (минимум 6 символов):");
            },
            OnResponse = async (manager, update) =>
            {
                var registerState = manager.GetUserState<RegisterUserState>(update.GetChatId());
                registerState.Password = update.GetMessageText();
                await manager.NotificationService.DeleteMessageAsync(update.GetChatId(), update.GetMessageId());
                return StepResultState.GoToNextStep;
            }
        };

        var askGroupIdStep = new FlowStep()
        {
            OnEnter = async (manager, conversation) =>
            {
                var events = await GetAllGroups(apiClient);
                await manager.NotificationService.SendTextMessageAsync(
                    conversation.ChatId,
                    "Выберите группу:", 
                    replyMarkup: events);
            },
            
            OnCallbackQuery = async (manager, update) =>
            {
                var state = manager.GetUserState<RegisterUserState>(update.GetChatId());
                
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
                
                state.GroupId = int.Parse(view.CallbackParam);
                
                return StepResultState.GoToNextStep;
            }
        };

        var confirmStep = new FlowStep
        {
            OnEnter = async (manager, state) =>
            {
                var registerState = (RegisterUserState)state;

                var group = await apiClient.GetGroup(registerState.GroupId);
                var groupName = group?.Name ?? "Неизвестная группа";

                var confirmationText = "Пожалуйста, проверьте ваши данные:\n\n" +
                                       $"<b>Имя:</b> {registerState.FirstName}\n" +
                                       $"<b>Фамилия:</b> {registerState.LastName}\n" +
                                       $"<b>Email:</b> {registerState.Email}\n" +
                                       $"<b>Группа:</b> {groupName}\n\n" +
                                       "Всё верно?";

                await manager.NotificationService.SendConfirmationAsync(
                    state.ChatId,
                    text: confirmationText,
                    yesCallback: "confirm_registration",
                    noCallback: "cancel_registration",
                    yesReply: "✅ Всё верно",
                    noReply: "❌ Отмена"
                );
            },
            OnCallbackQuery = async (manager, update) =>
            {
                var callbackData = update.GetCallbackText();

                if (callbackData == "confirm_registration")
                {
                    await manager.NotificationService.EditMessageTextAsync(
                        update.GetChatId(),
                        update.GetMessageId(),
                        text: "Данные подтверждены");

                    return StepResultState.GoToNextStep;
                }
                else // "cancel_registration"
                {
                    await manager.NotificationService.EditMessageTextAsync(
                        update.GetChatId(),
                        update.GetMessageId(),
                        text: "Регистрация отменена, /start");

                    return StepResultState.CancelFlow;
                }
            }
        };


        var finalStep = new FlowStep
        {
            OnEnter = async (manager, state) =>
            {
                var registerState = (RegisterUserState)state;
                
                registerState.TelegramUserId = state.UserId;

                var registrationDto = new RegisterUserDto(
                    registerState.FirstName,
                    registerState.LastName,
                    registerState.Email,
                    registerState.Password,
                    registerState.GroupId,
                    registerState.TelegramUsername,
                    state.UserId, 
                    null
                );

                var isSuccess = await apiClient.RegisterUser(registrationDto);

                if (isSuccess)
                {
                    await manager.NotificationService.SendTextMessageAsync(state.ChatId, "Регистрация прошла успешно!");
                    await manager.NotificationService.SendJoinQueueMenuAsync(state.ChatId);
                }
                else
                {
                    await manager.NotificationService.SendTextMessageAsync(state.ChatId,
                        $"Ошибка регистрации. \n\nНачните заново: /start");
                }
                
                manager.FinishConversation(state.ChatId);
            }
        };

        Steps =
        [
            askFirstNameStep,
            askLastNameStep,
            askEmailStep,
            askPasswordStep,
            askGroupIdStep,
            confirmStep,
            finalStep
        ];
    }
}