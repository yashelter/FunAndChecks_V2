using AdminBot.Models;
using FunAndChecks.DTO;
using FunAndChecks.Models.Enums;

namespace AdminBot.Services.Controllers;

public static class WrappedDataController
{
    public static List<WrappedData> Wrap(this List<GroupDto>? groups)
    {
        ArgumentNullException.ThrowIfNull(groups);
        return groups
            .OrderBy(group => group.Name)
            .Select(group =>
            new WrappedData()
            {
                GetId = () => group.Id.ToString(),
                GetString = () => group.Name,
            }).ToList();
    }
    
    public static List<WrappedData> Wrap(this List<SubjectDto>? subjects)
    {
        ArgumentNullException.ThrowIfNull(subjects);
        var lst = subjects.OrderByDescending(x => x.Id).ThenBy(x => x.Name);
        return lst.Select(s => new WrappedData()
        {
            GetId = () => s.Id.ToString(),
            GetString = () => s.Name,
        }).ToList();
    }
    
    public static List<WrappedData> Wrap(this List<TaskDto>? tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        var lst = tasks.OrderBy(x => x.Name).ThenBy(x => x.Id);
        
        return lst.Select(s => new WrappedData()
        {
            GetId = () => s.Id.ToString(),
            GetString = () => s.Name,
        }).ToList();
    }
    
    public static List<string>? Wrap(this List<TaskLog>? tasks)
    {
        return tasks?.Select(s => 
            $"Попытка от: <code>{s.SubmissionDate.ToShortDateString()}</code>\n" +
            $"Результат: <code>{s.Status.ToString()}</code>\n" +
            $"Принимал: <code>{(s.Admin is null ? "Ошибка (Можно считать бд сломана)" : s.Admin.Name)}</code>\n" +
            $"Комментарий: <blockquote>{s.Comment}</blockquote>\n"
        ).ToList();
    }
    
    public static List<WrappedData> Wrap(this List<TaskUserDto>? tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        var lst = tasks.OrderBy(x => x.Name).ThenBy(x => x.Id);
        
        return lst.Select(s => new WrappedData()
        {
            GetId = () => s.Id.ToString(),
            GetString = () => $"{s.Name} {GetStatus(s.Status)}",
        }).ToList();
    }

    private static string GetStatus(SubmissionStatus status)
    {
        return status switch
        {
            SubmissionStatus.NotSubmitted => "⚪",
            SubmissionStatus.Rejected => "❌",
            SubmissionStatus.Accepted => "✅",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
    
    
    public static List<WrappedData> UsersToWrappedData(this List<FullUserDto>? users)
    {
        ArgumentNullException.ThrowIfNull(users);
        return users.Select(user =>
            new WrappedData() 
            { 
                GetId = () => user.Id.ToString(), 
                GetString = () => $"{user.FirstName} {user.LastName}",
            }).ToList();
    }
    
    
    public static List<WrappedData> Wrap(this List<QueueEventDto>? queueEvents)
    {
        ArgumentNullException.ThrowIfNull(queueEvents);

        var someTimeAgo = DateTime.UtcNow - TimeSpan.FromDays(1);
        
        var filtered = queueEvents
            .Where(q => q.EventDateTime > someTimeAgo)
            .OrderBy(q => q.EventDateTime);
        
        return filtered.Select(qEvent => new WrappedData()
            {
                GetId = () => qEvent.Id.ToString(),
                GetString = () => $"{qEvent.EventDateTime.ToShortDateString()} -- {qEvent.Name}",
            })
            .ToList();
    }
}