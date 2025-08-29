using FunAndChecks.Models;

namespace FunAndChecks.Services;

public interface ITokenService
{
    string CreateToken(User user, IList<string> roles);
}