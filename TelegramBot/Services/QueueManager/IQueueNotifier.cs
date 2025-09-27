using FunAndChecks.DTO;
using TelegramBot.Models;

namespace TelegramBot.Services.QueueManager;


/// <summary>
/// Выполняет действия по оповещению об изменениях в очереди
/// </summary>
public interface IQueueNotifier
{
    /// <summary>
    /// Событие, которое вызывается при изменении очереди, на которую подписан пользователь.
    /// </summary>
    event Func<QueueSubscription, QueueUserUpdateDto, Task>? OnUpdate;
    
    /// <summary>
    /// Подписывает пользователя на обновления ОДНОЙ очереди.
    /// Если пользователь уже был подписан на другую очередь, старая подписка автоматически отменяется.
    /// </summary>
    /// <param name="newSubscription">Информация о новой подписке.</param>
    /// <returns>Объект созданной или обновленной подписки.</returns>
    Task<QueueSubscription> SubscribeUserToQueue(QueueSubscription newSubscription);
    
    /// <summary>
    /// Отменяет текущую активную подписку пользователя.
    /// </summary>
    /// <param name="userId">ID пользователя в Telegram.</param>
    Task UnsubscribeUserFromQueue(long userId);
    
    Task<bool> IsUserSubscribed(long userId);
    
    /// <summary>
    /// Находит и возвращает активную подписку пользователя.
    /// </summary>
    /// <param name="userId">ID пользователя Telegram.</param>
    /// <returns>
    /// Объект QueueSubscription, если подписка найдена.
    /// Null, если пользователь ни на что не подписан.
    /// </returns>
    Task<QueueSubscription?> GetSubscription(long userId);
}