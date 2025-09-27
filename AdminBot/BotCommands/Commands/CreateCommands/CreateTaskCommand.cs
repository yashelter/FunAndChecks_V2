using AdminBot.BotCommands.Flows;
using AdminBot.BotCommands.States;
using AdminBot.Conversations;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;
using Telegram.Bot.Types;

namespace AdminBot.BotCommands.Commands.CreateCommands;


public class CreateTaskCommand(IConversationManager conversationManager, IApiClient apiClient): IBotCommand
{
    public string Name { get; } = "/create_task";
    
    public async Task ExecuteAsync(Update update)
    {
        var flow = new CreateTaskFlow(apiClient);
        await conversationManager.StartFlowAsync(flow, new CreateTaskState()
        {
            ChatId = update.GetChatId(),
            UserId = update.GetUserId()
        });
    }
}