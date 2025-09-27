using Serilog;
using Telegram.Bot;
using TelegramBot;
using TelegramBot.BotCommands;
using TelegramBot.BotCommands.Commands.CreateCommands;
using TelegramBot.BotCommands.Commands.QueueCommands;
using TelegramBot.BotCommands.Flows;
using TelegramBot.BotCommands.Queue;
using TelegramBot.Conversations;
using TelegramBot.Services;
using TelegramBot.Services.ApiClient;
using TelegramBot.Services.Controllers;
using TelegramBot.Services.QueueManager;
using TelegramBot.Services.StateStorage;


Console.OutputEncoding = System.Text.Encoding.UTF8;


Console.OutputEncoding = System.Text.Encoding.UTF8;


using IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient("ApiV1", client =>
        {
            var apiConfig = hostContext.Configuration.GetSection("ApiConfiguration");
            client.BaseAddress = new Uri(apiConfig["BaseUrl"] ??
                                         throw new InvalidOperationException("Not set up your Api configuration."));
        });
        
        services.AddSingleton<ITelegramBotClient>(_ =>
        {
            var botToken = hostContext.Configuration["BotConfiguration:BotToken"];
            return string.IsNullOrEmpty(botToken)
                ? throw new InvalidOperationException("Bot Token is not configured.")
                :
                new TelegramBotClient(botToken);
        });
        services.AddHostedService<Worker>();

        services.AddSingleton<ITokenService, BotStateService>();
        services.AddSingleton<SignalRService>();
        services.AddSingleton<IQueueNotifier>(provider => provider.GetRequiredService<SignalRService>());
        services.AddHostedService(provider => provider.GetRequiredService<SignalRService>());
        
        services.AddSingleton<BotStateService>();
        services.AddSingleton<UpdateHandler>();
        services.AddSingleton<IConversationStore, InMemoryConversationStore>(); 
        
        services.AddScoped<IApiClient, ApiClient>();
        services.AddScoped<CommandRouter>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IConversationManager, ConversationManager>();
        services.AddScoped<IQueueController, QueueController>();
        
        
        // commands section
        services.AddScoped<IBotCommand, GetAllQueuesCommand>();
        services.AddScoped<IBotCommand, RegisterCommand>();
        services.AddScoped<IBotCommand, JoinQueueCommand>();
    })
    .Build();


host.Run();