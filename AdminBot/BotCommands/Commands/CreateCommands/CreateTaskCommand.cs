using AdminBot.BotCommands.Flows;
using AdminBot.Conversations;
using AdminBot.Services.Utils;
using Telegram.Bot.Types;

namespace AdminBot.BotCommands.Commands.CreateCommands;


public class CreateTaskCommand(IConversationManager conversationManager): IBotCommand
{
    public string Name { get; } = "/create_task";
    
    public async Task ExecuteAsync(Update update)
    {
        var flow = new CreateTaskFlow();
        await conversationManager.StartFlowAsync(flow, update.GetChatId(), update.GetUserId());
    }
}