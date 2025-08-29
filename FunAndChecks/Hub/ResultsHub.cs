namespace FunAndChecks.Hub;

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ResultsHub : Hub
{
    /// <summary>
    /// Метод для подписки клиента на обновления результатов по конкретному предмету.
    /// </summary>
    public async Task SubscribeToSubjectResults(int subjectId)
    {
        // Создаем уникальное имя группы для каждого предмета
        string groupName = $"results-subject-{subjectId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Метод для отписки.
    /// </summary>
    public async Task UnsubscribeFromSubjectResults(int subjectId)
    {
        string groupName = $"results-subject-{subjectId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}