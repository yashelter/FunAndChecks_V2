using AdminBot.BotCommands.Flows;
using AdminBot.Conversations;
using AdminBot.Services.Utils;
using Telegram.Bot.Types;

namespace AdminBot.BotCommands.Commands.CreateCommands;

public class CreateSubjectCommand(IConversationManager conversationManager) : IBotCommand
{
    public string Name => "/create_new_subject";

    public async Task ExecuteAsync(Update update)
    {
        var createSubjectFlow = new CreateSubjectFlow();
        await conversationManager.StartFlowAsync(createSubjectFlow, update.GetChatId(), update.GetUserId());
    }
}