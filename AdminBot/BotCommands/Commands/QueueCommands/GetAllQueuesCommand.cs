using AdminBot.BotCommands.Flows;
using AdminBot.Conversations;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Queue;
using AdminBot.Services.Utils;
using Telegram.Bot.Types;

namespace AdminBot.BotCommands.Commands.QueueCommands;

public class GetAllQueuesCommand(IQueueController controller, IApiClient api, IConversationManager conversationManager) : IBotCommand
{
    public string Name { get; } = "/get_all_queues";
    
    public async Task ExecuteAsync(Update update)
    {
        var flow = new GetAllQueuesFlow(controller, api);
        await conversationManager.StartFlowAsync(flow, update.GetChatId(), update.GetUserId());
    }
}