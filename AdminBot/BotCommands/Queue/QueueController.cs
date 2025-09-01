using AdminBot.BotCommands.Flows;
using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Models;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Keyboard;
using AdminBot.Services.QueueManager;
using FunAndChecks.DTO;
using FunAndChecks.Models.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using AdminBot.Services.Utils;
using static AdminBot.Services.Controllers.DataGetterController;
using static AdminBot.Utils.InputParser;


namespace AdminBot.BotCommands.Queue;

public class QueueController(
    INotificationService bot,
    ILogger<QueueController> logger,
    IApiClient apiClient,
    IConversationManager conversationManager,
    IQueueNotifier queueNotifier
    )
    : IQueueController
{
    
    public async Task<QueueSubscription> SubscribeToQueueEvent(long userId, int eventId)
    {
        var queue = await apiClient.GetQueueDetails(eventId);
        
        var participants = 
            queue?.Participants ??
            throw new ArgumentException($"Can't get queue {eventId}", nameof(eventId));
        
        var message = await bot.SendTextMessageAsync(userId, 
            "Очередь:",
            await RenderQueue(
                participants, 
                0, 
                $"queue_{eventId}")
        );

        var sub =  new QueueSubscription()
        {
            EventId = eventId,
            MessageId = message.MessageId,
            UserId = userId,
            EventName = $"{queue.EventDateTime.ToShortDateString()} -- {queue.EventName}",
            SubjectId = queue.SubjectId
        };
        
       return await queueNotifier.SubscribeUserToQueue(sub);
    }
    

    public async Task UpdateQueueStatus(QueueSubscription subscription, QueueUserUpdateDto update)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsUserSubscribed(long userId) => await queueNotifier.IsUserSubscribed(userId);

    public async Task HandleQueueCallbackAction(Update update)
    {
        if (update.CallbackQuery is null) throw new ArgumentException(null, nameof(update));
        var callback = update.GetCallbackText();
        
        // Формат: queue_2:01990041-ad21-792d-a63d-1d6c86063b19
        logger.LogInformation("Got {CallbackQueryMessage} in queue Handler", update.CallbackQuery.Message);

        var data = ParseQueueCallbackData(callback);

        if (data is null)
        {
            return;
        }
        
        int eventId = data.Value.EventId;
        string participantId = data.Value.ParticipantId;

        var sub = await queueNotifier.GetSubscription(update.GetUserId(), eventId);
        
        var flow = new CreateSubmissionFlow(apiClient);
        CreateSubmissionState state =  new CreateSubmissionState()
        {
            StudentId = participantId,
            SubjectId = sub.SubjectId,
            ChatId = update.GetChatId(),
            UserId = update.GetUserId(),
        };
        
        flow.AtEnd = async () =>
        {
            var subs = await SubscribeToQueueEvent(update.GetUserId(), eventId);
            var res = await queueNotifier.SubscribeUserToQueue(subs);
                
            await bot.EditMessageTextAsync(
                update.GetChatId(), 
                update.GetMessageId(),
                $"Очередь была отправлена новым сообщением",
                replyMarkup: null);
        };
        
        await conversationManager.StartFlowAsync(flow, state);
    }

    public async Task UnsubscribeUser(long userId)
    {
        throw new NotImplementedException();
    }


    private async Task<InlineKeyboardMarkup> RenderQueue(List<QueueParticipantDetailDto> queue, int page, string callback)
    {
        KeyboardGenerator.KeyboardSettings qSettings = new() { LineSize = 1,PageNumber = page} ;
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
            GetString = () => $"{GetQueueParticipantColor(item.Color)}{item.LastName} {item.FirstName} {GetQueueStatusEmote(item.Status)}"
        }));
        
        return wrapped;
    }
    
    
    private static string GetQueueStatusEmote(QueueUserStatus status) => status switch
    {
        QueueUserStatus.Checking  => "🎯",
        QueueUserStatus.Finished  => "🏳️",
        QueueUserStatus.Skipped   => "❌",
        QueueUserStatus.Waiting   => "⏳",
        _ => throw new ArgumentOutOfRangeException(nameof(status), $"Not expected status value: {status}"),
    };
    
    private static string GetQueueParticipantColor(string color) => color switch
    {
        "#E68800"  => "🟤 ",
        "#07FF00"  => "🟢 ",
        "#B67911"   => "🟤 ",
        "#1151B6"   => "🔵 ",
        _ => "",
    };
}



// ⚡📕📗📘📙♟️🎯⚔️📌🔝🏳️🏴