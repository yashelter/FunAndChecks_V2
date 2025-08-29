using AdminBot.Models;

namespace AdminBot.Services;

public interface ITokenService
{
    public void SaveUserTokenSession(UserSession session);
    
    public UserSession? GetUserTokenSession(long userId);
    
    public void DeleteUserTokenSession(long userId);

}