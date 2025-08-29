using LiteDB;
using TelegramBot.Models;

namespace TelegramBot.Services;

public class BotStateService: IDisposable
{
    private readonly ILiteDatabase _db = new LiteDatabase(@"..\bot_storage.db");

    private ILiteCollection<UserState> States =>  _db.GetCollection<UserState>("states");
    private ILiteCollection<UserSession> Sessions => _db.GetCollection<UserSession>("sessions");
    
    
    public void SaveUserSession(UserSession session)
    {
        Sessions.Upsert(session);
        _db.Checkpoint();
    }

    public UserSession? GetUserSession(long userId)
    {
        return Sessions.FindById(userId);
    }

    public void ResetUserState(UserState state)
    {
        state.BlackBox.Clear();
        state.State = ConversationState.None;
        SetUserState(state);
    }
    
    public void ResetUserState(long userId)
    {
        ResetUserState(GetUserState(userId));
    }
    
    public void SetUserState(UserState state)
    {
        States.Upsert(state);
        _db.Checkpoint(); // todo: delete this
    }

    public UserState GetUserState(long userId)
    {
        return States.FindById(userId) ??
               new UserState { UserId = userId, State = ConversationState.None };
    }

    void IDisposable.Dispose()
    {
        _db.Dispose();
    }
}