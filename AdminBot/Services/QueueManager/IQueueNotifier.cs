using AdminBot.Models;
using FunAndChecks.DTO;

namespace AdminBot.Services.QueueManager;


/// <summary>
/// Выполняет действия по оповещению об изменениях в очереди
/// </summary>
public interface IQueueNotifier
{
    /// <summary>
    /// Событие, которое вызывается при изменении очереди
    /// </summary>
    public event Func<QueueSubscription, QueueUserUpdateDto, Task>? OnUpdate;
    
    /// <summary>
    /// Выполняет подписку пользователя на обновления.
    /// При обновлении вызывает событие <see cref="OnUpdate"/>
    /// </summary>
    /// <param name="newSubscription"></param>
    /// <returns></returns>
    Task<QueueSubscription> SubscribeUserToQueue(QueueSubscription newSubscription);
    
    /// <summary>
    /// Выполняет отмену подписки пользователя на обновления всех событий
    /// </summary>
    /// <param name="userId">ID пользователя в Telegram</param>
    /// <returns></returns>
    Task UnsubscribeUserFromQueue(long userId);

    /// <summary>
    /// Выполняет отмену подписки пользователя на обновления одного события
    /// </summary>
    /// <param name="userId">ID пользователя в Telegram</param>
    /// <param name="eventId">ID события очереди</param>
    /// <returns></returns>
    Task UnsubscribeUserFromQueue(long userId, int eventId);
    
    
    /// <summary>
    /// Проверяет подписан ли пользователь на обновления очереди
    /// </summary>
    /// <param name="userId">ID пользователя в Telegram</param>
    /// <returns></returns>
    Task<bool> IsUserSubscribed(long userId);
    
    /// <summary>
    /// Находит и возвращает активную подписку пользователя на конкретное событие очереди.
    /// </summary>
    /// <param name="userId">ID пользователя Telegram</param>
    /// <param name="eventId">ID события очереди</param>
    /// <returns>
    /// Объект QueueSubscription, если подписка найдена.
    /// Null, если пользователь не подписан на это событие.
    /// </returns>
    Task<QueueSubscription?> GetSubscription(long userId, int eventId);
}