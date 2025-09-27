using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.BotCommands;
using TelegramBot.BotCommands.Queue;
using TelegramBot.Conversations;
using TelegramBot.Utils;

namespace TelegramBot.Services.Controllers;


/// <summary>
/// 
/// </summary>
public class CommandRouter
{
    private readonly ILogger<CommandRouter> _logger;
    private readonly Dictionary<string, IBotCommand> _commands;
    private readonly IConversationManager _conversationManager;
    private readonly IQueueController _queueController;
    private readonly INotificationService _bot;
    

    public CommandRouter(IEnumerable<IBotCommand> commands,
        IConversationManager conversationManager,
        ILogger<CommandRouter> logger,
        IQueueController queueController,
        INotificationService notificationService)
    {
        _commands = commands.ToDictionary(c => c.Name, c => c);
        _conversationManager =  conversationManager;
        _logger = logger;
        _queueController = queueController;
        _bot = notificationService;
    }

    /// <summary>
    /// Общий цикл работы:<br/>
    /// Если находимся в потоке, любой ответ делегируется обработчику потока  <see cref="IConversationManager"/> <br/>
    /// Если, если есть подписка на очередь, и тип сообщения CallbackQuery, то
    /// делегируется  обработчику очереди <see cref="IQueueController"/><br/>
    /// Иначе делегируется обработчику команд <see cref="IBotCommand"/>
    /// </summary>
    /// <param name="update">Обновление из Telegram Api (новое сообщение или callback)</param>
    public async Task ProcessCommand(Update update)
    {
        var chatId = update.GetChatId();
        var userId = update.GetUserId();
        
        // Команда позволяющая сбросить состояние из произвольного состояния, учитывая малое тестирование
        // является необходимостью. В дальнейшем конечно, нужно избавиться от потенциальных softlock'ов
        if (update is { Type: UpdateType.Message, Message.Text: "/reset" })
        {
            await _queueController.ResetUserState(userId);
            await _conversationManager.ResetUserState(userId);
            _logger.LogInformation("Reset user state for user {UserId}", userId);
        }
        
        if (await _conversationManager.IsUserInConversationAsync(chatId))
        {
            await _conversationManager.ProcessResponseAsync(update);
            return;
        }

        // TODO: в теории можно проверять, смог ли контроллер обработать действие
        
        // значит действие должно быть из очереди (если очередь создаст поток, мы попадём выше)
        if (await _queueController.IsUserSubscribed(userId) && update.Type == UpdateType.CallbackQuery)
        {
            await _queueController.HandleQueueCallbackAction(update);
            return;
        }
        
        // прилетело сообщение вне потока, и при активной подписке, дабы не усложнять - отменяем подписку
        if (await _queueController.IsUserSubscribed(userId) && update.Type == UpdateType.Message)
        {
            await _queueController.ResetUserState(userId);
        }
        
        
        var messageText = update.Message?.Text;
        if (string.IsNullOrEmpty(messageText)) return;
        

        string commandName = messageText.Split(' ')[0].ToLower();
        if (_commands.TryGetValue(commandName, out var command))
        {
            await command.ExecuteAsync(update);
        }
    }
}