using AdminBot.BotCommands.Flows;
using AdminBot.Conversations;
using AdminBot.Services.Utils;
using Telegram.Bot.Types;

namespace AdminBot.BotCommands.Commands.CreateCommands;

public class LinkGroupToSubjectCommand(IConversationManager conversationManager): IBotCommand
{
    public string Name { get; } = "/link_group_to_subject";
    
    public async Task ExecuteAsync(Update update)
    {
        var flow = new LinkGroupToSubjectFlow();
        await conversationManager.StartFlowAsync(flow, update.GetChatId(), update.GetUserId());
    }
}