using AdminBot.BotCommands;
using AdminBot.Conversations;
using AdminBot.Services.Controllers;
using AdminBot.Services.Utils;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace AdminBot.Services;

public class UpdateHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UpdateHandler> _logger;
    
    public UpdateHandler(IServiceProvider serviceProvider, ILogger<UpdateHandler> logger)
    {
        _serviceProvider =  serviceProvider;
        _logger = logger;
    }
    
    public async Task HandleUpdateAsync(Update update, CancellationToken ct)
    {
        
        await using var scope = _serviceProvider.CreateAsyncScope();

        try
        {
            //var conversationManager = scope.ServiceProvider.GetRequiredService<IConversationManager>();
            var commandRouter = scope.ServiceProvider.GetRequiredService<CommandRouter>();

            await commandRouter.ProcessCommand(update);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing update {UpdateId}", update.Id);
            // Здесь же можно получить INotificationService из scope и отправить сообщение об ошибке пользователю
        }
       
    }
    
    public Task HandlePollingErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError("Polling error: {ErrorMessage}", errorMessage);
        return Task.CompletedTask;
    }
}