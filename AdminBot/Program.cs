using AdminBot;
using AdminBot.BotCommands;
using AdminBot.BotCommands.Commands.CreateCommands;
using AdminBot.BotCommands.Commands.QueueCommands;
using AdminBot.BotCommands.Flows;
using AdminBot.Conversations;
using AdminBot.Services;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Controllers;
using AdminBot.Services.Queue;
using Serilog;
using Telegram.Bot;

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
        services.AddSingleton<IQueueManager>(provider => provider.GetRequiredService<SignalRService>());
        services.AddHostedService(provider => provider.GetRequiredService<SignalRService>());
        
        services.AddSingleton<BotStateService>();
        services.AddSingleton<UpdateHandler>();
        
        services.AddScoped<IApiClient, ApiClient>();
        services.AddScoped<CommandRouter>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IConversationManager, ConversationManager>();
        services.AddScoped<IQueueController, QueueController>();
        
        // flows section
        services.AddScoped<CreateSubjectFlow>();
        services.AddScoped<GetAllQueuesFlow>();
        services.AddScoped<CreateQueueEventFlow>();
        services.AddScoped<CreateGroupFlow>();
        
        // commands section
        services.AddScoped<IBotCommand, CreateSubjectCommand>();
        services.AddScoped<IBotCommand, GetAllQueuesCommand>();
        services.AddScoped<IBotCommand, CreateQueueEventCommand>();
        services.AddScoped<IBotCommand, CreateGroupCommand>();

        
    })
    .Build();


host.Run();