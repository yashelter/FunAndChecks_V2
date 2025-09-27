using Telegram.Bot.Types;
using TelegramBot.BotCommands.Flows;
using TelegramBot.BotCommands.Queue;
using TelegramBot.Conversations;
using TelegramBot.Models;
using TelegramBot.Services.ApiClient;
using TelegramBot.Utils;

namespace TelegramBot.BotCommands.Commands.QueueCommands;

public class GetAllQueuesCommand(
    IConversationManager conversationManager,
    IQueueController queueController,
    IApiClient apiClient) : IBotCommand
{
    public string Name { get; } = "/get_all_queues";
    
    public async Task ExecuteAsync(Update update)
    {
        var flow = new GetAllQueuesFlow(apiClient, queueController);
        await conversationManager.StartFlowAsync(flow, new ConversationState()
        {
            ChatId = update.GetChatId(),
            UserId = update.GetUserId()
        });
        
    }
}