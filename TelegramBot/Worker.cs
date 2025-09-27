using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBot.Services;

namespace TelegramBot;

using Telegram.Bot;
using Telegram.Bot.Polling;

public class Worker(ILogger<Worker> logger, ITelegramBotClient botClient, UpdateHandler updateHandler)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var me = await botClient.GetMe(stoppingToken);
        logger.LogInformation("Bot {Username} started", me.Username);
        
        botClient.StartReceiving(
            updateHandler: (bot, update, ct) => updateHandler.HandleUpdateAsync(update, ct),
            errorHandler: (bot, ex, ct) => updateHandler.HandlePollingErrorAsync(ex, ct),
            receiverOptions: new ReceiverOptions { AllowedUpdates = [] },
            cancellationToken: stoppingToken
        );
    }
}