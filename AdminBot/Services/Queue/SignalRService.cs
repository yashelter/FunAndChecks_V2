using System.Collections.Concurrent;
using AdminBot.Models;
using FunAndChecks.DTO;
using Microsoft.AspNetCore.SignalR.Client;

namespace AdminBot.Services.Queue;


/// <summary>
/// Сервис следит за update и подписками на обновления,
/// если пользователь подписан
/// И при этом оповещает "IQueueController", при Update
/// </summary>
public class SignalRService : IQueueManager, IHostedService
{
    private readonly ILogger<SignalRService> _logger;
    private readonly HubConnection _connection;
    private readonly IServiceProvider _serviceProvider;

    private readonly ConcurrentDictionary<int, ConcurrentBag<QueueSubcription>> _subscriptions = new();

    public SignalRService(IConfiguration configuration,
        ILogger<SignalRService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

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

    
    public async Task SubscribeUserToQueue(long userId, int eventId)
    {
        var subscribers = _subscriptions.GetOrAdd(eventId, []);

        if (subscribers.All(s => s.UserId != userId))
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var queue = scope.ServiceProvider.GetRequiredService<IQueueController>();
            subscribers.Add(await queue.SubscribeToQueueEvent(userId, eventId));
        }

        if (subscribers.Count == 1 && _connection.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync("SubscribeToQueue", eventId);
            _logger.LogInformation("Bot subscribed to queue {EventId} on behalf of user {UserId}", eventId, userId);
        }
    }

    public async Task UnsubscribeUserFromQueue(long userId, int eventId)
    {
        if (_subscriptions.TryGetValue(eventId, out var subscribers))
        {
            var newSubscribers = new ConcurrentBag<QueueSubcription>(subscribers.Where(q => q.UserId != userId));
            _subscriptions[eventId] = newSubscribers;

            if (newSubscribers.IsEmpty && _connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("UnsubscribeFromQueue", eventId);
                _logger.LogInformation("Bot unsubscribed from queue {EventId}", eventId);
            }
        }
    }


    private async Task HandleQueueUserUpdate(QueueUserUpdateDto update)
    {
        _logger.LogInformation("Received queue update for UserId: {UserId}, NewStatus: {NewStatus}", update.UserId,
            update.NewStatus);

        if (_subscriptions.TryGetValue(update.EventId, out var subscribers))
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var queue = scope.ServiceProvider.GetRequiredService<IQueueController>();

            foreach (var sub in subscribers)
            {
                await queue.UpdateQueueStatus(sub, update);
            }
        }
    }

    
    public Task<bool> IsUserSubscribed(long userId)
    {
        var listeningAnyQueue =
            _subscriptions.Any(kp =>
                kp.Value.Any(s => s.UserId == userId));

       return Task.FromResult(listeningAnyQueue);
    }

    
    public Task<bool> IsUserSubscribed(long userId, long eventId)
    {
        var listeningQueue =
            _subscriptions.Any(kp => kp.Key == eventId &&
                kp.Value.Any(s => s.UserId == userId));

        return Task.FromResult(listeningQueue);
    }
}