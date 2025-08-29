namespace FunAndChecks.DTO;


public record UserInfoDto(
    Guid Id, string FirstName, string LastName, string Email, string? GroupName);

public record LinkTelegramDto(long TelegramId);
