using System.Collections.Concurrent;
using AdminBot.BotCommands.Queue;
using AdminBot.Models;
using FunAndChecks.DTO;
using Microsoft.AspNetCore.SignalR.Client;

namespace AdminBot.Services.QueueManager;


/// <summary>
/// Сервис следит за update и подписками на обновления, если пользователь подписан
/// Не должен (согласно задуманному дизайну) использоваться нигде, кроме <see cref="IQueueController"/>
/// Является Singleton
/// </summary>
public class SignalRService : IQueueNotifier, IHostedService
{
    public event Func<QueueSubscription, QueueUserUpdateDto, Task>? OnUpdate;
    
    private readonly ILogger<SignalRService> _logger;
    private readonly HubConnection _connection;

    private readonly ConcurrentDictionary<int, ConcurrentDictionary<long, QueueSubscription>> _subscriptions = new();

    public SignalRService(IConfiguration configuration,
        ILogger<SignalRService> logger)
    {
        _logger = logger;

        var hubUrl = configuration["ApiConfiguration:BaseUrl"]?.TrimEnd('/') + "/queueHub";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();
        _connection.On<QueueUserUpdateDto>("QueueUserUpdated", HandleQueueUserUpdate);
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _connection.StartAsync(cancellationToken);
            _logger.LogInformation("SignalR connection started.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to SignalR hub.");
        }
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SignalR connection stopping.");
        return _connection.DisposeAsync().AsTask();
    }

    
    public async Task UnsubscribeUserFromQueue(long userId, int eventId)
    {
        if (_subscriptions.TryGetValue(eventId, out var subscribersForEvent) 
            && subscribersForEvent.TryRemove(userId, out _))
        {
            _logger.LogInformation("User {UserId} unsubscribed from queue {EventId}.", userId, eventId);
            
            if (subscribersForEvent.IsEmpty && _connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("UnsubscribeFromQueue", eventId);
                _logger.LogInformation("Bot unsubscribed from queue {EventId} on server.", eventId);
            }
        }
    }
    
    
    public async Task<QueueSubscription> SubscribeUserToQueue(QueueSubscription newSubscription)
    {
        var subscribersForEvent = _subscriptions.GetOrAdd(newSubscription.EventId,
            _ => new ConcurrentDictionary<long, QueueSubscription>());
        
        var addedOrUpdatedSubscription = subscribersForEvent.AddOrUpdate(
            key: newSubscription.UserId,
            addValue: newSubscription,
            updateValueFactory: (_, existingSubscription) =>
            {
                existingSubscription.MessageId = newSubscription.MessageId;
                existingSubscription.EventName = newSubscription.EventName;
                return existingSubscription;
            }
        );


        if (subscribersForEvent.Count != 1 || !subscribersForEvent.ContainsKey(newSubscription.UserId))
            return addedOrUpdatedSubscription;

        await _connection.InvokeAsync("SubscribeToQueue", newSubscription.EventId);
        
        _logger.LogInformation(
            "Bot subscribed to queue {EventId} on behalf of the first user {UserId}",
            newSubscription.EventId,
            newSubscription.UserId);

        return addedOrUpdatedSubscription;
    }

    public async Task UnsubscribeUserFromQueue(long userId)
    {
        var eventsToCheckForCleanup = new List<int>();

        foreach (var eventId in _subscriptions.Keys)
        {
            if (!_subscriptions.TryGetValue(eventId, out var subscribersForEvent) ||
                !subscribersForEvent.TryRemove(userId, out _)) continue;
            
            _logger.LogInformation("User {UserId} unsubscribed from queue {EventId}.", userId, eventId);
                    
            if (subscribersForEvent.IsEmpty)
            {
                eventsToCheckForCleanup.Add(eventId);
            }
        }

        if (eventsToCheckForCleanup.Count != 0 && _connection.State == HubConnectionState.Connected)
        {
            foreach (var eventId in eventsToCheckForCleanup)
            {
                try
                {
                    await _connection.InvokeAsync("UnsubscribeFromQueue", eventId);
                    _logger.LogInformation("Bot unsubscribed from queue {EventId} on server as there are no more listeners.", eventId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to unsubscribe from queue {EventId} on server.", eventId);
                }
            }
        }
    }


    private async Task HandleQueueUserUpdate(QueueUserUpdateDto update)
    {
        _logger.LogInformation("Received queue update for UserId: {UserId}, NewStatus: {NewStatus}", update.UserId,
            update.NewStatus);

        if (_subscriptions.TryGetValue(update.EventId, out var subscribers))
        {
            foreach (var sub in subscribers)
            {
                if (OnUpdate == null)
                {
                    _logger.LogWarning("Not set up any OnUpdate Handlers, Queue will don't update it.");
                    return;
                }
                foreach (var handler in
                         OnUpdate.GetInvocationList().Cast<Func<QueueSubscription, QueueUserUpdateDto, Task>>())
                {
                    await handler(sub.Value, update);
                }
            }
        }
    }


    public Task<bool> IsUserSubscribed(long userId)
    {
        var listeningAnyQueue =
            _subscriptions.Any(kp =>
                kp.Value.Any(s => s.Value.UserId == userId));

        return Task.FromResult(listeningAnyQueue);
    }
    

    public Task<QueueSubscription?> GetSubscription(long userId, int eventId)
    {
        if (_subscriptions.TryGetValue(eventId, out var subscribersForEvent) 
            && subscribersForEvent.TryGetValue(userId, out var subscription))
        {
            return Task.FromResult<QueueSubscription?>(subscription);
        }
        return Task.FromResult<QueueSubscription?>(null);
    }
}