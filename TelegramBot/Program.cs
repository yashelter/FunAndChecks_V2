using Serilog;
using TelegramBot;
using TelegramBot.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = Host.CreateApplicationBuilder(args);


IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    })
    .ConfigureServices((hostContext, services) =>
    {
        
        
        services.AddHttpClient("ApiV1", client =>
        {
            var apiConfig = hostContext.Configuration.GetSection("ApiConfiguration");
            client.BaseAddress = new Uri(apiConfig["BaseUrl"]);
        });
        services.AddScoped<IApiClient, ApiClient>();
        services.AddSingleton<UpdateHandler>();
        services.AddHostedService<Worker>();
        services.AddSingleton<BotStateService>();
    })
    .Build();


host.Run();