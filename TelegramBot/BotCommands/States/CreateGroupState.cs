using TelegramBot.Models;

namespace TelegramBot.BotCommands.States;

public class RegisterUserState: ConversationState
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int GroupId { get; set; }
    public string TelegramUsername { get; set; }
    public long? TelegramUserId { get; set; }
    public string? GitHubUrl { get; set; }
}