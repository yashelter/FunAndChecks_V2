using AdminBot.Models;
using FunAndChecks.DTO;
using Telegram.Bot.Types;


namespace AdminBot.Services.Queue;


public interface IQueueController
{
    /// <summary>
    /// Должно отправить сообщение с очередью
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="eventId"></param>
    /// <returns></returns>
    Task<QueueSubcription> SubscribeToQueueEvent(long userId, int eventId);
    
    /// <summary>
    /// Обновляет сообщение об очереди
    /// </summary>
    /// <param name="subscription"></param>
    /// <param name="update"></param>
    /// <returns></returns>
    Task UpdateQueueStatus(QueueSubcription subscription, QueueUserUpdateDto update);
    

    Task HandleQueueCallbackAction(Update update);

    Task HandleNewQueueSubscription(int userId, int queueId);

}