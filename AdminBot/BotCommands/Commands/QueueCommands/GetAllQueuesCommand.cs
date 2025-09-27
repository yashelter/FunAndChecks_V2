using AdminBot.BotCommands.Flows;
using AdminBot.BotCommands.Queue;
using AdminBot.Conversations;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;
using Telegram.Bot.Types;

namespace AdminBot.BotCommands.Commands.QueueCommands;

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