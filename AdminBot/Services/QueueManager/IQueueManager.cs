using AdminBot.Models;

namespace AdminBot.Services.QueueManager;


public interface IQueueManager
{
    Task<QueueSubscription> SubscribeUserToQueue(QueueSubscription newSubscription);
    Task UnsubscribeUserFromQueue(long userId, int eventId);
    
    Task<bool> IsUserSubscribed(long userId);
    
    Task<bool> IsUserSubscribed(long userId, long eventId);

    /// <summary>
    /// Находит и возвращает активную подписку пользователя на конкретное событие очереди.
    /// </summary>
    /// <param name="userId">ID пользователя Telegram.</param>
    /// <param name="eventId">ID события очереди.</param>
    /// <returns>
    /// Объект QueueSubscription, если подписка найдена.
    /// Null, если пользователь не подписан на это событие.
    /// </returns>
    Task<QueueSubscription?> GetSubscription(long userId, int eventId);

}