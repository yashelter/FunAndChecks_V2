using Telegram.Bot.Types;
using TelegramBot.BotCommands.Flows;
using TelegramBot.BotCommands.States;
using TelegramBot.Conversations;
using TelegramBot.Models;
using TelegramBot.Services.ApiClient;
using TelegramBot.Utils;

namespace TelegramBot.BotCommands.Commands.CreateCommands;

public class RegisterCommand(IApiClient apiClient, IConversationManager conversationManager): IBotCommand
{
    public string Name { get; } = "/register";
    
    public async Task ExecuteAsync(Update update)
    {
        var flow = new RegisterFlow(apiClient);
        await conversationManager.StartFlowAsync(flow, new RegisterUserState()
        {
            ChatId = update.GetChatId(),
            UserId = update.GetUserId()
        });
    }
}