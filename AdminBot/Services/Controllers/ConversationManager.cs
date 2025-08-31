using AdminBot.Conversations;
using AdminBot.Models;
using AdminBot.Services.ApiClient;
using AdminBot.Services.Utils;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace AdminBot.Services.Controllers;


public class ConversationManager(
    BotStateService botStateService,
    IServiceProvider serviceProvider,
    INotificationService notificationService,
    IApiClient apiClient)
    : IConversationManager
{
    public INotificationService NotificationService { get; } = notificationService;
    public IApiClient ApiClient { get; } = apiClient;
    

    public async Task StartFlowAsync(ConversationFlow flow, long chatId, long userId, ConversationState? initialState = null)
    {
        var state = initialState ?? flow.CreateStateObject(chatId, userId);
        
        var session = new ConversationSession
        {
            ChatId = chatId,
            UserId = userId,
            FlowTypeName = flow.GetType().FullName ?? throw new InvalidOperationException("Can't get flow type name"),
            CurrentStepIndex = 0,
            StateJson = JsonConvert.SerializeObject(state)
        };
        botStateService.SaveSession(session);
        
        var firstStep = flow.Steps[0];
        if (firstStep.OnEnter != null)
        {
            await firstStep.OnEnter(this, state);
        }
    }
    

    public async Task ProcessResponseAsync(Update update)
    {
        long userId = update.GetUserId();
        
        var session = botStateService.GetSession(update.GetUserId());
        if (session == null) return;
        
        var flow = GetFlowByTypeName(session.FlowTypeName);
        
        if (flow == null)
        {
            await NotificationService.SendTextMessageAsync(update.GetChatId(), "Произошла внутренняя ошибка. Диалог сброшен.");
            botStateService.DeleteSession(userId);
            return;
        }

        var currentStep = flow.Steps[session.CurrentStepIndex];

        StepResult result = new() { State = StepResultState.RepeatStep };
        
        if (update.Message != null && currentStep.OnResponse != null)
        {
            result = await currentStep.OnResponse(this, update);
        }
        else if (update.CallbackQuery != null && currentStep.OnCallbackQuery != null)
        {
            result = await currentStep.OnCallbackQuery(this, update);
        }
        
        await ApplyStepResult(result, session, flow);
    }
    
    private async Task ApplyStepResult(StepResult result, ConversationSession session, ConversationFlow flow)
    {
        var step = flow.Steps[session.CurrentStepIndex];

        if (result.ResultingState is not null)
        {
            UpdateUserState(result.ResultingState);
            session = botStateService.GetSession(result.ResultingState.UserId) ??
                      throw new InvalidOperationException($"Can't get flow state for {result.ResultingState.UserId}");;
        }
        
        switch (result.State)
        {
            case StepResultState.GoToNextStep:
                session.CurrentStepIndex++;
                botStateService.SaveSession(session);

                if (session.CurrentStepIndex < flow.Steps.Count)
                {
                    var nextStep = flow.Steps[session.CurrentStepIndex];
                    if (nextStep.OnEnter != null)
                    {
                        var state = flow.CreateStateObject(session.ChatId, session.UserId);
                        await nextStep.OnEnter(this, state);
                    }
                }
                else
                {
                    botStateService.DeleteSession(session.UserId);
                }
                break;
                
            case StepResultState.FinishFlow:
            case StepResultState.CancelFlow:
                botStateService.DeleteSession(session.UserId);
                break;
                
            case StepResultState.RepeatStep:
                if (step.OnEnter != null)
                {
                    var state = flow.CreateStateObject(session.ChatId, session.UserId);
                    await step.OnEnter(this, state);
                }
                break;
            case StepResultState.Nothing:
                break;
        }
    }

    public Task<bool> IsUserInConversationAsync(long userId)
    {
        var session = botStateService.GetSession(userId);
        return Task.FromResult(session != null);
    }

    public T GetUserState<T>(long userId) where T : ConversationState, new()
    {
        var session = botStateService.GetSession(userId);
        if (session == null)
        {
            throw new InvalidOperationException("No active conversation session found for this user.");
        }
        
        return JsonConvert.DeserializeObject<T>(session.StateJson) ?? new T();
    }

    
    private void UpdateUserState(ConversationState state)
    {
        var session = botStateService.GetSession(state.UserId);
        if (session != null)
        {
            session.StateJson = JsonConvert.SerializeObject(state);
            botStateService.SaveSession(session);
        }
    }

    
    private ConversationFlow? GetFlowByTypeName(string typeName)
    {
        var flowType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.FullName == typeName && typeof(ConversationFlow).IsAssignableFrom(t));

        if (flowType == null) return null;
        
        return serviceProvider.GetService(flowType) as ConversationFlow ?? Activator.CreateInstance(flowType) as ConversationFlow;
    }
}