using AdminBot.BotCommands.Flows;
using AdminBot.Conversations;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;
using Telegram.Bot.Types;

namespace AdminBot.BotCommands.Commands.CreateCommands;

public class CreateGroupCommand(IConversationManager conversationManager) : IBotCommand
{
    public string Name { get; } = "/create_group";
    public async Task ExecuteAsync(Update update)
    {
        var flow = new CreateGroupFlow();
        await conversationManager.StartFlowAsync(flow, update.GetChatId(), update.GetUserId());
    }
}