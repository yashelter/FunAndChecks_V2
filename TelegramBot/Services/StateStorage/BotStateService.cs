using LiteDB;
using TelegramBot.Models;

namespace TelegramBot.Services.StateStorage;


public sealed class BotStateService : ITokenService, IDisposable
{
    private readonly ILiteDatabase _db;

    public BotStateService(IConfiguration configuration)
    {
        // var connectionString = configuration.GetConnectionString("BotStorage"); // TODO
        _db = new LiteDatabase("database.db");
    }

    private ILiteCollection<UserSession> UserSessions => _db.GetCollection<UserSession>("user_sessions");


    public void SaveUserTokenSession(UserSession session) => UserSessions.Upsert(session);
    
    public UserSession? GetUserTokenSession(long userId) => UserSessions.FindById(userId);
    
    public void DeleteUserTokenSession(long userId) => UserSessions.Delete(userId);
    

    void IDisposable.Dispose()
    {
        _db.Dispose();
    }
}