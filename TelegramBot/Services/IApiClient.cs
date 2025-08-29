using FunAndChecks.DTO;

namespace TelegramBot.Services;

public interface IApiClient
{
    /// <summary>
    /// Пытается войти в систему, используя логин и пароль.
    /// </summary>
    /// <returns>JWT токен в случае успеха, иначе null.</returns>
    Task<string?> LoginAsync(string username, string password);

    /// <summary>
    /// Пытается войти в систему беспарольно, используя Telegram ID.
    /// </summary>
    /// <returns>JWT токен в случае успеха, иначе null.</returns>
    Task<string?> TelegramLoginAsync(long telegramId);

    /// <summary>
    /// Привязывает Telegram ID к текущему пользователю (определяется по токену).
    /// </summary>
    /// <returns>True в случае успеха.</returns>
    Task<bool> LinkTelegramAccountAsync(long telegramId, string userToken);

    Task<List<GroupDto>?> GetAllGroups();

    Task<(bool IsSuccess, string? ErrorMessage)> RegisterUserAsync(RegisterUserDto registrationDto);

}