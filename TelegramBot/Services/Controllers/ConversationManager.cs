using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Conversations;
using TelegramBot.Models;
using TelegramBot.Services.StateStorage;
using TelegramBot.Utils;

namespace TelegramBot.Services.Controllers;


public class ConversationManager(
    INotificationService notificationService,
    ILogger<ConversationManager> logger,
    IConversationStore storage)
    : IConversationManager
{
    public INotificationService NotificationService { get; } = notificationService;


    public async Task StartFlowAsync(ConversationFlow flow, ConversationState initialState)
    {
        long userId = initialState.UserId;
        long chatId = initialState.ChatId;
        
        if (storage.ContainsKey(userId))
        {
            await NotificationService.SendTextMessageAsync(chatId, 
                "Попытка создать вложенный поток [DBG]");
            logger.LogWarning("Already active conversation {UserId}", userId);
            throw new InvalidOperationException("Conversation already started");
        }
        
        var session = new ActiveConversationSession
        {
            ChatId = chatId,
            UserId = userId,
            Flow = flow,
            CurrentStepIndex = 0,
            State = initialState
        };

        storage.AddOrUpdate(userId, session);
        
        var firstStep = flow.Steps[0];
        if (firstStep.OnEnter != null)
        {
            await firstStep.OnEnter(this, session.State);
        }
    }
    

    public async Task ProcessResponseAsync(Update update)
    {
        long userId = update.GetUserId();

        if (!storage.TryGetValue(userId, out var session))
        {
            // TODO:
            return;
        }

        var flow = session.Flow;

        var currentStep = flow.Steps[session.CurrentStepIndex];
        StepResultState resultState = StepResultState.Nothing;
        
        if (update.Message != null && currentStep.OnResponse != null)
        {
            resultState = await currentStep.OnResponse(this, update);
        }
        else if (update.CallbackQuery != null && currentStep.OnCallbackQuery != null)
        {
            resultState = await currentStep.OnCallbackQuery(this, update);
        }
        
        await ApplyStepResult(resultState, session, flow);
    }
    
    private async Task ApplyStepResult(StepResultState resultState, ActiveConversationSession session, ConversationFlow flow)
    {
        var step = flow.Steps[session.CurrentStepIndex];
        
        switch (resultState)
        {
            case StepResultState.GoToNextStep:
                session.CurrentStepIndex++;

                if (session.CurrentStepIndex < flow.Steps.Count)
                {
                    var nextStep = flow.Steps[session.CurrentStepIndex];
                    if (nextStep.OnEnter != null)
                    {
                        await nextStep.OnEnter(this, session.State);
                    }
                }
                else
                {
                    logger.LogInformation("Flow for user {UserId} finished by reaching the end of steps.", session.UserId);
                    FinishConversation(session.UserId);
                }
                break;
                
            case StepResultState.FinishFlow:
            case StepResultState.CancelFlow:
                FinishConversation(session.UserId);
                break;
                
            case StepResultState.RepeatStep:
                if (step.OnEnter != null)
                {
                    await step.OnEnter(this, session.State);
                }
                break;
            case StepResultState.Nothing:
                break;
        }
    }
    public void FinishConversation(long userId)
    {
        storage.TryRemove(userId, out _);
    }

   

    public Task<bool> IsUserInConversationAsync(long userId)
    {
        var session = storage.ContainsKey(userId);
        return Task.FromResult(session);
    }

    public T GetUserState<T>(long userId) where T : ConversationState
    {
        if (!storage.TryGetValue(userId, out var session))
        {
            throw new InvalidOperationException($"No active conversation found for user {userId}.");
        }

        if (session.State is T typedState)
        {
            return typedState;
        }
        throw new InvalidCastException($"The current conversation state is of type '{session.State.GetType().Name}', but type '{typeof(T).Name}' was requested.");
    }
    
    public async Task ResetUserState(long userId)
    {
        storage.TryRemove(userId, out _);
        await notificationService.SendTextMessageAsync(userId, "Состояние потоков было сброшено");
    }
}