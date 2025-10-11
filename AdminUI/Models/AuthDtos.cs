namespace AdminUI.Models;

public record RegisterUserDto(
    string FirstName, 
    string LastName, 
    string Email, 
    string Password, 
    int GroupId,
    string TelegramUsername,
    long? TelegramUserId,
    string? GitHubUrl);
public record LoginUserDto(string TelegramUsername, string Password);
public record AuthResponseDto(string Token);

public record TelegramLoginDto(long TelegramId, string AuthHash);


