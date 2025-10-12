namespace AdminUI.Models;

public class RegisterUserDto
{
    public RegisterUserDto()
    {
    }

    public RegisterUserDto(string firstName, 
        string lastName, 
        string email, 
        string password, 
        int? groupId,
        string telegramUsername,
        long? telegramUserId,
        string? gitHubUrl)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
        GroupId = groupId;
        TelegramUsername = telegramUsername;
        TelegramUserId = telegramUserId;
        GitHubUrl = gitHubUrl;
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int? GroupId { get; set; }
    public string TelegramUsername { get; set; }
    public long? TelegramUserId { get; set; }
    public string? GitHubUrl { get; set; }
    
}

public record LoginUserDto(string TelegramUsername, string Password);
public record AuthResponseDto(string Token);

public record TelegramLoginDto(long TelegramId, string AuthHash);


