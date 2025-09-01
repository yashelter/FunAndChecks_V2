using AdminBot.BotCommands.Flows;
using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;
using Telegram.Bot.Types;

namespace AdminBot.BotCommands.Commands.CreateCommands;

public class LinkGroupToSubjectCommand(IConversationManager conversationManager, IApiClient apiClient): IBotCommand
{
    public string Name { get; } = "/link_group_to_subject";
    
    public async Task ExecuteAsync(Update update)
    {
        var flow = new LinkGroupToSubjectFlow(apiClient);
        await conversationManager.StartFlowAsync(flow, new LinkGroupToSubjectState()
        {
            ChatId = update.GetChatId(),
            UserId = update.GetUserId()
        });
    }
}