using LiteDB;

namespace TelegramBot.Models;

// Хранит долгоживущий JWT токен
public class UserSession
{
    // Id будет равен Telegram UserId
    [BsonId] public long UserId { get; set; } 
    public string JwtToken { get; set; }
    public DateTime CreatedAt { get; set; }
}
