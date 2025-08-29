using LiteDB;

namespace AdminBot.Models;

public class UserSession
{
    [BsonId]
    public long UserId { get; set; }
    
    public string? JwtToken { get; set; }
}