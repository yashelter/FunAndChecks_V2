namespace TelegramBot.Models;

public class MessageSuccess
{
    public MessageSuccess(bool success, string? message = null)
    {
        IsSuccess = success;
        Message = message;
    }
    public bool IsSuccess {get; set;}
    public string? Message {get; set;}
}