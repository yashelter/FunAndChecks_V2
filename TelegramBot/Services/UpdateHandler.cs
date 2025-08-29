using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Text;
using FunAndChecks.DTO;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Models;
using TelegramBot.Utils;

namespace TelegramBot.Services;

public class UpdateHandler(BotStateService botState,  IServiceProvider serviceProvider, ILogger<UpdateHandler> logger)
{
    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Polling error");
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        long userId = update?.Message?.From?.Id
                      ?? update?.CallbackQuery?.From?.Id
                      ?? update?.InlineQuery?.From?.Id
                      ?? update?.ChosenInlineResult?.From?.Id
                      ?? update?.PreCheckoutQuery?.From?.Id
                      ?? update?.ShippingQuery?.From?.Id
                      ?? throw new InvalidDataException("Invalid try to get user id");
        
        var userState = botState.GetUserState(userId);

        
        if (update.Type == UpdateType.CallbackQuery)
        {
            await HandleCallbackQuery(botClient, update, userId, userState);
        }
        
        if (update.Type != UpdateType.Message || update.Message is not { Text: { } messageText }) return;

        long? chatId = update.Message?.Chat.Id;
        logger.LogInformation("Received a '{messageText}' message in chat {chatId}.", messageText, chatId);


        // there only Awaiting commands + some extra like reset
        var action = userState.State switch
        {
            ConversationState.None               => HandleCommand(botClient, update, userId, userState),
            ConversationState.AwaitingRegister   => HandleRegister(botClient, update, userId, userState),
            ConversationState.AwaitingLogin      => HandleLogin(botClient, update, userId, userState),
            //ConversationState.AwaitingPassword   => HandlePasswordInput(botClient, update, userId),
            //    _                                    => HandleUnknownState(botClient, userId)
        };
        
        await action;
        
    }

    private async Task HandleCallbackQuery(ITelegramBotClient botClient, Update update, long userId, UserState state)
    {
        var callback = update.CallbackQuery;
        var callbackData = update.CallbackQuery.Data;
        
        var chatId = callback.Message.Chat.Id;
        
        logger.LogInformation("Received callback_query with data: {callbackData}", callbackData);

        if (callbackData == "/cancel")
        {
            botState.ResetUserState(state);
            
            await botClient.EditMessageText(
                chatId: chatId,
                messageId: update.Message.Id,
                replyMarkup: null,
                text: "Операция отменена"
            );
        }
        else if (callbackData == "/done")
        {
            // hint, here should get only "Pending" state, by definition
            var action = state.State switch
            {
                ConversationState.PendingRegister   => MakeRegister(state),
                ConversationState.PendingLogin      => MakeLogin(state),

                //    _  => HandleUnknownState(botClient, userId)
            };
        
            var result = await action;
            await botClient.EditMessageText(
                chatId: chatId,
                messageId: update.Message.Id,
                text:  update.Message.Text + "\n\n" + 
                       (result.IsSuccess ? 
                    $"<blockquote>Действие успешно выполнено: {result.Message}</blockquote" : 
                    $"<blockquote>Возникла ошибка: {result.Message}</blockquote"),
                replyMarkup: null,
                parseMode: ParseMode.Html
            );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="userId"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    private Task HandleCommand(ITelegramBotClient botClient, Update update, long userId, UserState state)
    {
        string command = update.Message?.Text ?? string.Empty;
        string normalizedCommand = command.ToLower();
        
        if (normalizedCommand == "/register")
        {
            state.State = ConversationState.AwaitingRegister;
            state.UserName = update.Message!.From!.Username!;
            botState.SetUserState(state);
            return botClient.SendMessage(userId, "Введите вашу фамилию:");
        } if (normalizedCommand == "/login")
        {
            state.State = ConversationState.AwaitingLogin;
            state.UserName = update.Message!.From!.Username!;
            botState.SetUserState(state);
            // todo: for far future, should also write login, because it's not necessary telegram login
            return botClient.SendMessage(userId, "Введите ваш пароль:");
        }
        
        // ... другие команды
        return Task.CompletedTask;
    }

    private Task HandleLogin(
        ITelegramBotClient botClient,
        Update update,
        long userId,
        UserState state)
    {
        string? input = update.Message?.Text ?? null;
        input = input?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            return botClient.SendMessage(userId, "Ввод не корректен, повторите попытку");
        }
        
        if (state.BlackBox.TryAdd("password", input))
        {
            state.State = ConversationState.PendingLogin;
            botState.SetUserState(state);
            
            var inlineKeyboard = new InlineKeyboardMarkup([
                [
                    InlineKeyboardButton.WithCallbackData(text: "✅ Всё верно", callbackData: "/done"),
                    InlineKeyboardButton.WithCallbackData(text: "❌ Отмена", callbackData: "/cancel")
                ]
            ]);
            
            return botClient.SendMessage(userId, 
                $"username: <code>{state.UserName}</code>,\n" +
                $"пароль: <code>{state.BlackBox["password"]}</code>",
                parseMode: ParseMode.Html,
                replyMarkup:  inlineKeyboard);
        } 
        return botClient.SendMessage(userId, "Надо нажать кнопочку");
    }

    
    private Task HandleRegister(
        ITelegramBotClient botClient, 
        Update update, 
        long userId, 
        UserState state)
    {
        string? input = update.Message?.Text ?? null;
        input = input?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            return botClient.SendMessage(userId, "Ввод не корректен, повторите попытку");
        }
        
        if (state.BlackBox.TryAdd("firstname", input.ToProperNameCase()))
        {
            botState.SetUserState(state);
            return botClient.SendMessage(userId, "Введите фамилию");
        } 
        if (!state.BlackBox.ContainsKey("lastname"))
        {
            state.BlackBox["lastname"] = input.ToProperNameCase();
            botState.SetUserState(state);
            return botClient.SendMessage(userId, "Введите ваш email (желательно к которому есть доступ)");
        }
        if (state.BlackBox.TryAdd("email", input))
        {
            botState.SetUserState(state);
            return botClient.SendMessage(userId, "Введите пароль (min 8 символов)");
        }
        using var scope = serviceProvider.CreateScope();
        var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();
        
        if (!state.BlackBox.ContainsKey("password"))
        {
            if (input.Length < 8)
            {
                return botClient.SendMessage(userId, "Научись читать пж. Введите пароль (min >>>8<<<< символов)");
            }
            state.BlackBox["password"] = input;
            var groups = from g in apiClient.GetAllGroups().Result select g.Name;
            botState.SetUserState(state);
            return botClient.SendMessage(userId, 
                "Выберите группу: (будьте внимательны, и желательно уточните у старосты на всякий)," +
                "если её вдруг нет, то можете вписать текстом",
                replyMarkup: groups.ToArray());
        }
        else if (!state.BlackBox.ContainsKey("groupid"))
        {
            var id = from g in apiClient.GetAllGroups().Result where g.Name == input select g.Id;
            var enumerable = id as int[] ?? id.ToArray();
            if (!enumerable.Any())
            {
                return botClient.SendMessage(userId, "Такой группы нет, попробуй ещё");
            }
            state.BlackBox["groupid"] = enumerable.First().ToString();
            state.State = ConversationState.PendingRegister;
            botState.SetUserState(state);
            
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "✅ Всё верно", callbackData: "/done"),
                    InlineKeyboardButton.WithCallbackData(text: "❌ Отмена", callbackData: "/cancel"),
                },
            });
            
            return botClient.SendMessage(userId, 
                $"Итого: \n" +
                $"Имя: <code>{state.BlackBox["firstname"]}</code>\n," +
                $"фамилия <code>{state.BlackBox["lastname"]}</code>\n" +
                $"почта <code>{state.BlackBox["email"]}</code>\n" +
                $"tg username: <code>{state.UserName}</code>,\n" +
                $"пароль: <code>{state.BlackBox["password"]}</code>,\n" +
                $"группа: <code>{input}</code> [id {state.BlackBox["groupid"]}]\n" +
                $"Если, что'\'-то неверно, то /cancel, и регистрируйся заново\n" +
                $"Если всё верно, подтверди командой /done",
                parseMode: ParseMode.Html,
                replyMarkup:  inlineKeyboard);
        }
        return botClient.SendMessage(userId, "Надо нажать кнопочку");
    }

    private async Task<MessageSuccess> MakeRegister(UserState state)
    {
        using var scope = serviceProvider.CreateScope();
        var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();
        var box = state.BlackBox;
        var (success, errorMessage) = await apiClient.RegisterUserAsync(
            new RegisterUserDto(box["firstname"],
            box["lastname"],
            box["email"],
            box["password"],
            Convert.ToInt32(box["groupid"]),
            state.UserName,
            state.UserId, null));

        if (success)
        {
            await LoginUser(state.UserName, box["password"], state);
            return new MessageSuccess(true, "Успешная регистрация");
        }
        return new MessageSuccess(false, errorMessage);
    }
    
    private async Task<MessageSuccess> MakeLogin(UserState state)
    {
        try
        {
            await LoginUser(state.UserName, state.BlackBox["password"], state);
            return new MessageSuccess(true, "Успешный вход");
        }
        catch (AuthenticationException ex)
        {
            return new MessageSuccess(false, ex.Message);
        }
    }
    
    
    private async Task LoginUser(
        string login,
        string password,
        UserState state)
    {
        
        using var scope = serviceProvider.CreateScope();
        var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();
        
        var token = await apiClient.LoginAsync(login, password);
        if (token is null) throw new AuthenticationException("Ошибка входа, возможно неверные данные");
        await LinkAccounts(state, token);
        
    }

    private async Task LinkAccounts(UserState state, string token)
    {
        using var scope = serviceProvider.CreateScope();
        var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();

        botState.SaveUserSession(new UserSession { UserId = state.UserId, JwtToken = token });
        var result = await apiClient.LinkTelegramAccountAsync(state.UserId, token);
        botState.ResetUserState(state);

        // if (!result) throw new AuthenticationException("Failed to link telegram account");
        
    }

}

/* todo:
create subject
create group
link group-subject
create task to subject
create queue
watch queue
--> github repo
edit queue -> выставить задачи, закончить проверку\скипнуть
pick student -> выставить задачи
----
student: 
register to queue
watch queue?
watch own stats
----
pages:
watching results
----
all in docker
auto back-up every day
*/