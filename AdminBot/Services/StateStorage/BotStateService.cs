using AdminBot.Models;
using LiteDB;

namespace AdminBot.Services;


public sealed class BotStateService : ITokenService, IDisposable
{
    private readonly ILiteDatabase _db;

    public BotStateService(IConfiguration configuration)
    {
        // var connectionString = configuration.GetConnectionString("BotStorage"); // TODO
        _db = new LiteDatabase("database.db");
    }

    private ILiteCollection<ConversationSession> Sessions => _db.GetCollection<ConversationSession>("conversation_sessions");
    private ILiteCollection<UserSession> UserSessions => _db.GetCollection<UserSession>("user_sessions");


    public void SaveUserTokenSession(UserSession session) => UserSessions.Upsert(session);
    
    public UserSession? GetUserTokenSession(long userId) => UserSessions.FindById(userId);
    
    public void DeleteUserTokenSession(long userId) => UserSessions.Delete(userId);
    
    public void SaveSession(ConversationSession session)
    {
        Sessions.Upsert(session);
    }

    public ConversationSession? GetSession(long userId)
    {
        return Sessions.FindById(userId);
    }

    public void DeleteSession(long userId)
    {
        Sessions.Delete(userId);
    }

    void IDisposable.Dispose()
    {
        _db.Dispose();
    }
}