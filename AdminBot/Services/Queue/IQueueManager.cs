namespace AdminBot.Services.Queue;


public interface IQueueManager
{
    Task SubscribeUserToQueue(long userId, int eventId);
    
    Task UnsubscribeUserFromQueue(long userId, int eventId);
    
    Task<bool> IsUserSubscribed(long userId);
    
    Task<bool> IsUserSubscribed(long userId, long eventId);
}