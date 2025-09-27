using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using TelegramBot.BotCommands.Flows;
using TelegramBot.BotCommands.Queue;
using TelegramBot.BotCommands.States;
using TelegramBot.Conversations;
using TelegramBot.Models;
using TelegramBot.Services.ApiClient;
using TelegramBot.Utils;

namespace TelegramBot.BotCommands.Commands.QueueCommands;

public class JoinQueueCommand (
    IConversationManager conversationManager,
    IQueueController queueController,
    IApiClient apiClient) : IBotCommand
{
    public string Name { get; } = "/join_queue";
    
    public async Task ExecuteAsync(Update update)
    {
        var flow = new JoinQueueFlow(apiClient);
        await conversationManager.StartFlowAsync(flow, new JoinQueueState()
        {
            ChatId = update.GetChatId(),
            UserId = update.GetUserId()
        });
        
    }
}