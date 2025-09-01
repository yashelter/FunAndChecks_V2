using AdminBot.BotCommands.Flows;
using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;
using Telegram.Bot.Types;

namespace AdminBot.BotCommands.Commands.CreateCommands;

public class CreateSubjectCommand(IConversationManager conversationManager, IApiClient apiClient) : IBotCommand
{
    public string Name => "/create_new_subject";

    public async Task ExecuteAsync(Update update)
    {
        var flow = new CreateSubjectFlow(apiClient);
        await conversationManager.StartFlowAsync(flow, new CreateSubjectState()
        {
            ChatId = update.GetChatId(),
            UserId = update.GetUserId()
        });
    }
}