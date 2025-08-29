using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBot.Services;

namespace TelegramBot;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly UpdateHandler _updateHandler; 

    public Worker(ILogger<Worker> logger, IConfiguration configuration, UpdateHandler updateHandler)
    {
        _logger = logger;
        _updateHandler = updateHandler;
        var botToken = configuration["BotConfiguration:BotToken"];
        _botClient = new TelegramBotClient(botToken!);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var me = await _botClient.GetMe(stoppingToken);
        _logger.LogInformation("Bot {Username} started", me.Username);

        _botClient.StartReceiving(
            updateHandler: _updateHandler.HandleUpdateAsync,
            errorHandler: _updateHandler.HandlePollingErrorAsync,
            receiverOptions: new ReceiverOptions { AllowedUpdates = [] },
            cancellationToken: stoppingToken
        );
    }
}