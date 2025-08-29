using System.Collections.Concurrent;
using LiteDB;

namespace TelegramBot.Models;

public class UserState
{
    [BsonId] public long UserId { get; set; } // non resets
    public string UserName { get; set; } // non resets (can think as active after first login)
    public ConversationState State { get; set; }
    
    public ConcurrentDictionary<string, string> BlackBox { get; set; } = new();

}

