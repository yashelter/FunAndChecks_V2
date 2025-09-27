using FunAndChecks.DTO;
using FunAndChecks.Models.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Conversations;
using TelegramBot.Models;
using TelegramBot.Services.ApiClient;
using TelegramBot.Services.Keyboard;
using TelegramBot.Services.QueueManager;
using TelegramBot.Utils;
using static TelegramBot.Utils.InputParser;


namespace TelegramBot.BotCommands.Queue;

public class QueueController : IQueueController
{
    private readonly INotificationService _bot;
    private readonly ILogger<QueueController> _logger;
    private readonly IApiClient _apiClient;
    private readonly IConversationManager _conversationManager;
    private readonly IQueueNotifier _queueNotifier;

    public QueueController(INotificationService bot,
        ILogger<QueueController> logger,
        IApiClient apiClient,
        IConversationManager conversationManager,
        IQueueNotifier queueNotifier)
    {
        _bot = bot;
        _logger = logger;
        _apiClient = apiClient;
        _conversationManager = conversationManager;
        _queueNotifier = queueNotifier;

        _queueNotifier.OnUpdate += UpdateQueueStatus;
    }

    public async Task<QueueSubscription> SubscribeToQueueEvent(long userId, int eventId)
    {
        var queue = await _apiClient.GetQueueDetails(eventId);

        var participants =
            queue?.Participants ??
            throw new ArgumentException($"Can't get queue {eventId}", nameof(eventId));

        var message = await _bot.SendTextMessageAsync(userId,
            "Очередь:",
            await RenderQueue(
                participants,
                0,
                $"queue_{eventId}")
        );

        var sub = new QueueSubscription()
        {
            EventId = eventId,
            MessageId = message.MessageId,
            UserId = userId,
            EventName = $"{queue.EventDateTime.ToShortDateString()} -- {queue.EventName}",
            SubjectId = queue.SubjectId
        };

        return await _queueNotifier.SubscribeUserToQueue(sub);
    }


    // TODO:
    // Можно работать только с update,
    // А не заново получать всю очередь, но для этого и сервер чуть надо менять,
    // в общем good first issue
    public async Task UpdateQueueStatus(QueueSubscription subscription, QueueUserUpdateDto update)
    {
        // Те измениться должна вот эта строка ↓↓↓
        var queue = await _apiClient.GetQueueDetails(subscription.EventId);

        var participants =
            queue?.Participants ??
            throw new ArgumentException($"Can't get queue {subscription}", nameof(subscription));

        await _bot.EditMessageTextAsync(
            subscription.UserId,
            subscription.MessageId,
            "Очередь:",
            await RenderQueue(
                participants,
                0,
                $"queue_{subscription.EventId}")
        );
    }

    public async Task<bool> IsUserSubscribed(long userId) => await _queueNotifier.IsUserSubscribed(userId);

    public async Task HandleQueueCallbackAction(Update update)
    {
        if (update.CallbackQuery is null) throw new ArgumentException(null, nameof(update));
        var callback = update.GetCallbackText();

        // Формат: queue_2:01990041-ad21-792d-a63d-1d6c86063b19
        _logger.LogInformation("Got {CallbackQueryMessage} in queue Handler", update.CallbackQuery.Message);

        var data = ParseQueueCallbackData(callback);

        if (data is null)
        {
            return;
        }

        int eventId = data.Value.EventId;
        string participantId = data.Value.ParticipantId;

        var userId = update.GetUserId();
        var chatId = update.GetChatId();

        var sub = await _queueNotifier.GetSubscription(userId);
        if (sub == null) return;

        var queueDetails = await _apiClient.GetQueueDetails(eventId);
        var participant = queueDetails?.Participants
            .FirstOrDefault(p => p.UserId.ToString() == participantId);

        if (participant == null)
        {
            await _bot.SendTextMessageAsync(chatId, "Не удалось найти участника. Возможно, очередь обновилась.");
            return;
        }
        // Idk what to do for now
    }

    public async Task UnsubscribeUser(long userId)
    {
        await _queueNotifier.UnsubscribeUserFromQueue(userId);
    }

    public async Task ResetUserState(long userId)
    {
        await UnsubscribeUser(userId);
        await _bot.SendTextMessageAsync(userId, "Подписки на очереди были сброшены");

    }


    private async Task<InlineKeyboardMarkup> RenderQueue(List<QueueParticipantDetailDto> queue, int page,
        string callback)
    {
        KeyboardGenerator.KeyboardSettings qSettings = new() { LineSize = 1, PageNumber = page };
        var wrapped = WrapQueue(queue);

        return KeyboardGenerator.GenerateKeyboardPage(wrapped, callback, qSettings);

    }

    private List<WrappedData> WrapQueue(List<QueueParticipantDetailDto> queue)
    {
        List<WrappedData> wrapped = [];

        var sortedQueue = queue
            .OrderByDescending(q => q.Status)
            .ThenBy(q => q.TotalPoints)
            .ThenBy(q => q.LastName);

        wrapped.AddRange(sortedQueue.Select(item => new WrappedData()
        {
            GetId = () => item.UserId.ToString(),
            GetString = () =>
                $"{GetQueueParticipantColor(item.Color)}{item.LastName} {item.FirstName} {GetQueueStatusEmote(item.Status)}"
        }));

        return wrapped;
    }


    private static string GetQueueStatusEmote(QueueUserStatus status) => status switch
    {
        QueueUserStatus.Checking => "🎯",
        QueueUserStatus.Finished => "🏳️",
        QueueUserStatus.Skipped => "❌",
        QueueUserStatus.Waiting => "⏳",
        _ => throw new ArgumentOutOfRangeException(nameof(status), $"Not expected status value: {status}"),
    };

    private static string GetQueueParticipantColor(string color) => color switch
    {
        "#E68800" => "🟤 ",
        "#07FF00" => "🟢 ",
        "#B67911" => "🟤 ",
        "#1151B6" => "🔵 ",
        _ => "",
    };
}



// ⚡📕📗📘📙♟️🎯⚔️📌🔝🏳️🏴