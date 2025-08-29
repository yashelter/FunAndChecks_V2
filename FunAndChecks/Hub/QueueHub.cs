namespace FunAndChecks.Hub;

using Microsoft.AspNetCore.SignalR;

public class QueueHub : Hub
{
    /// <summary>
    /// Метод, который клиент (бот) будет вызывать для подписки на обновления конкретной очереди.
    /// </summary>
    public async Task SubscribeToQueue(int eventId)
    {
        string groupName = $"queue-{eventId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Метод для отписки (не обязательно, но хорошая практика).
    /// </summary>
    public async Task UnsubscribeFromQueue(int eventId)
    {
        string groupName = $"queue-{eventId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}