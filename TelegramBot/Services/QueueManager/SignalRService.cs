using System.Collections.Concurrent;
using FunAndChecks.DTO;
using Microsoft.AspNetCore.SignalR.Client;
using TelegramBot.BotCommands.Queue;
using TelegramBot.Models;

namespace TelegramBot.Services.QueueManager;


/// <summary>
/// Сервис следит за update и подписками на обновления, если пользователь подписан
/// Не должен (согласно задуманному дизайну) использоваться нигде, кроме <see cref="IQueueController"/>
/// Является Singleton
/// </summary>
///
public class SignalRService : IQueueNotifier, IHostedService
{
    public event Func<QueueSubscription, QueueUserUpdateDto, Task>? OnUpdate;
    
    private readonly ILogger<SignalRService> _logger;
    private readonly HubConnection _connection;

    private readonly ConcurrentDictionary<long, QueueSubscription> _userSubscriptions = new();
    private readonly ConcurrentDictionary<int, int> _eventSubscriberCount = new();
    
    
    public SignalRService(IConfiguration configuration, ILogger<SignalRService> logger)
    {
        _logger = logger;
        var hubUrl = configuration["ApiConfiguration:BaseUrl"]?.TrimEnd('/') + "/queueHub";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();
        _connection.On<QueueUserUpdateDto>("QueueUserUpdated", HandleQueueUserUpdate);
    }
    

    public async Task<QueueSubscription> SubscribeUserToQueue(QueueSubscription newSubscription)
    {
        if (_userSubscriptions.TryGetValue(newSubscription.UserId, out var oldSubscription))
        {
            if (oldSubscription.EventId == newSubscription.EventId)
            {
                oldSubscription.MessageId = newSubscription.MessageId;
                _userSubscriptions[newSubscription.UserId] = oldSubscription;
                return oldSubscription;
            }
            
            await UnsubscribeUserFromQueue(newSubscription.UserId);
        }

        _userSubscriptions[newSubscription.UserId] = newSubscription;
        int newCount = _eventSubscriberCount.AddOrUpdate(newSubscription.EventId, 1, (_, count) => count + 1);

        if (newCount == 1 && _connection.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync("SubscribeToQueue", newSubscription.EventId);
            _logger.LogInformation("Bot subscribed to queue {EventId} (first listener).", newSubscription.EventId);
        }

        _logger.LogInformation("User {UserId} subscribed to queue {EventId}.", newSubscription.UserId, newSubscription.EventId);
        return newSubscription;
    }

    public async Task UnsubscribeUserFromQueue(long userId)
    {
        if (_userSubscriptions.TryRemove(userId, out var removedSubscription))
        {
            _logger.LogInformation("User {UserId} unsubscribed from queue {EventId}.", userId, removedSubscription.EventId);
            
            int newCount = _eventSubscriberCount.AddOrUpdate(removedSubscription.EventId, 0, (_, count) => count - 1);

            if (newCount <= 0)
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    await _connection.InvokeAsync("UnsubscribeFromQueue", removedSubscription.EventId);
                    _logger.LogInformation("Bot unsubscribed from queue {EventId} (last listener).", removedSubscription.EventId);
                }
                _eventSubscriberCount.TryRemove(removedSubscription.EventId, out _);
            }
        }
    }

    public Task<bool> IsUserSubscribed(long userId)
    {
        var t = _userSubscriptions.TryGetValue(userId, out _);
        return Task.FromResult(t);
    }
    
    private Task HandleQueueUserUpdate(QueueUserUpdateDto update)
    {
        if (OnUpdate == null)
        {
            _logger.LogWarning("OnUpdate event has no subscribers. Queue update will be ignored.");
            return Task.CompletedTask;
        }
        
        foreach (var userSubscription in _userSubscriptions.Values)
        {
            if (userSubscription.EventId == update.EventId)
            {
                _ = OnUpdate.Invoke(userSubscription, update);
            }
        }

        return Task.CompletedTask;
    }

    public Task<QueueSubscription?> GetSubscription(long userId)
    {
        _userSubscriptions.TryGetValue(userId, out var subscription);
        return Task.FromResult(subscription);
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        int maxRetries = 5;
        var retryDelay = TimeSpan.FromSeconds(5);

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await _connection.StartAsync(cancellationToken);
                _logger.LogInformation("SignalR connection started successfully.");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to SignalR hub on attempt {AttemptNumber}. Retrying in {Delay} seconds...", i + 1, retryDelay.TotalSeconds);
            
                if (i == maxRetries - 1)
                {
                    _logger.LogError(ex, "Could not connect to SignalR hub after {MaxRetries} attempts.", maxRetries);
                    break;
                }
            
                await Task.Delay(retryDelay, cancellationToken);
            }
        }
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SignalR connection stopping.");
        return _connection.DisposeAsync().AsTask();
    }
}