using AdminBot.BotCommands.Flows;
using AdminBot.Conversations;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;
using Telegram.Bot.Types;

namespace AdminBot.BotCommands.Commands.CreateCommands;

public class CreateQueueEventCommand(IConversationManager conversationManager, IApiClient apiClient): IBotCommand
{
    public string Name { get; } = "/create_queue_event";
    
    public async Task ExecuteAsync(Update update)
    {
        CreateQueueEventFlow flow = new CreateQueueEventFlow(apiClient);
        await conversationManager.StartFlowAsync(flow, update.GetChatId(), update.GetUserId());

    }
}