using AdminBot.BotCommands;
using AdminBot.Conversations;
using AdminBot.Services.Queue;
using AdminBot.Services.Utils;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AdminBot.Services.Controllers;


/// <summary>
/// 
/// </summary>
public class CommandRouter
{
    private readonly ILogger<CommandRouter> _logger;
    private readonly Dictionary<string, IBotCommand> _commands;
    private readonly IConversationManager _conversationManager;
    private readonly IQueueManager _queueManager;
    private readonly IQueueController _queueController;
    

    public CommandRouter(IEnumerable<IBotCommand> commands,
        IConversationManager conversationManager,
        ILogger<CommandRouter> logger,
        IQueueManager queueManager, 
        IQueueController queueController)
    {
        _commands = commands.ToDictionary(c => c.Name, c => c);
        _conversationManager =  conversationManager;
        _logger = logger;
        _queueManager = queueManager;
        _queueController = queueController;
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

        if (await _conversationManager.IsUserInConversationAsync(chatId))
        {
            await _conversationManager.ProcessResponseAsync(update);
            return;
        }


        // TODO: в теории можно проверять, смог ли контроллер обработать действие
        if (await _queueManager.IsUserSubscribed(userId) && update.Type == UpdateType.CallbackQuery)
        {
            await _queueController.HandleQueueCallbackAction(update);
            return;
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