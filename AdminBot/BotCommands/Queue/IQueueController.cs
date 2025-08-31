using AdminBot.Models;
using FunAndChecks.DTO;
using Telegram.Bot.Types;

namespace AdminBot.BotCommands.Queue;


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
    

    Task HandleQueueCallbackAction(Update update);

    Task HandleNewQueueSubscription(long userId, int queueId);

}