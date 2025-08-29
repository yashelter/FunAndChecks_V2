using AdminBot.Conversations;
using AdminBot.Models;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Keyboard;
using FunAndChecks.DTO;
using FunAndChecks.Models.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.Services.Queue;

public class QueueController(
    INotificationService bot,
    ILogger<QueueController> logger,
    IApiClient apiClient
    )
    : IQueueController
{
    
    public async Task<QueueSubcription> SubscribeToQueueEvent(long userId, int eventId)
    {
        var message = await bot.SendTextMessageAsync(userId, 
            "Очередь:",
            await RenderQueue(
                (await apiClient.GetQueueDetails(eventId))?.Participants ?? 
                throw new ArgumentException($"Can't get queue {eventId}", nameof(eventId)), 
                1, 
                $"queue_{eventId}")
        );

        return new QueueSubcription()
        {
            EventId = eventId,
            MessageId = message.MessageId,
            UserId = userId,
        };
    }
    

    public async Task UpdateQueueStatus(QueueSubcription subscription, QueueUserUpdateDto update)
    {
        throw new NotImplementedException();
    }

    public async Task HandleQueueCallbackAction(Update update)
    {
        if (update.Type != UpdateType.CallbackQuery || update.CallbackQuery is null) throw new ArgumentException(nameof(update));
        
        logger.LogInformation("Got {CallbackQueryMessage} in queue Handler", update.CallbackQuery.Message);
        
        
        throw new NotImplementedException();
    }
    
    
    public async Task HandleNewQueueSubscription(int userId, int queueId)
    {
      //  bot.SendTextMessageAsync(userId, "")
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

    
    private string GetQueueStatusEmote(QueueUserStatus status) => status switch
    {
        QueueUserStatus.Checking  => "🎯",
        QueueUserStatus.Finished  => "🏳️",
        QueueUserStatus.Skipped   => "❌",
        QueueUserStatus.Waiting   => "⏳",
        _ => throw new ArgumentOutOfRangeException(nameof(status), $"Not expected status value: {status}"),
    };
    
    private string GetQueueParticipantColor(string color) => color switch
    {
        "#E68800"  => "🟤 ",
        "#07FF00"  => "🟢 ",
        "#B67911"   => "🟤 ",
        "#1151B6"   => "🔵 ",
        _ => "",
    };
}



// ⚡📕📗📘📙♟️🎯⚔️📌🔝🏳️🏴