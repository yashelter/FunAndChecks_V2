using AdminBot.BotCommands.Flows;
using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;
using Telegram.Bot.Types;

namespace AdminBot.BotCommands.Commands.CreateCommands;

public class CreateGroupCommand(IConversationManager conversationManager, IApiClient apiClient) : IBotCommand
{
    public string Name { get; } = "/create_group";
    
    public async Task ExecuteAsync(Update update)
    {
        var flow = new CreateGroupFlow(apiClient);
        await conversationManager.StartFlowAsync(flow, new CreateGroupState()
        {
            ChatId = update.GetChatId(),
            UserId = update.GetUserId()
        });
    }
}