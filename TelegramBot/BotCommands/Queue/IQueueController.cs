using FunAndChecks.DTO;
using Telegram.Bot.Types;
using TelegramBot.Models;

namespace TelegramBot.BotCommands.Queue;


public interface IQueueController
{
    /// <summary>
    /// Должно отправить сообщение с очередью
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="eventId"></param>
    /// <returns></returns>
    Task<QueueSubscription> SubscribeToQueueEvent(long userId, int eventId);
    
    /// <summary>
    /// Обновляет сообщение об очереди
    /// </summary>
    /// <param name="subscription"></param>
    /// <param name="update"></param>
    /// <returns></returns>
    Task UpdateQueueStatus(QueueSubscription subscription, QueueUserUpdateDto update);
    
    Task<bool> IsUserSubscribed(long userId);
    
    
    Task HandleQueueCallbackAction(Update update);
    
    // Есть случай что повторно запрашиваются очереди, тогда отменим подписку и дадим подписаться на другое
    // В других случаях просто отправим новое сообщение очереди
    Task UnsubscribeUser(long userId);
    
    Task ResetUserState(long userId);
}