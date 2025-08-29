using AdminBot.Models;
using FunAndChecks.DTO;

namespace AdminBot.Services.Controllers;

public static class WrappedDataController
{
    public static List<WrappedData> GroupsToWrappedData(this List<GroupDto>? groups)
    {
        ArgumentNullException.ThrowIfNull(groups);
        return groups.Select(group =>
            new WrappedData() { GetId = () => group.Id.ToString(), GetString = () => group.Name, }).ToList();
    }
    
    public static List<WrappedData> WrapSubjects(this List<SubjectDto>? subjects)
    {
        ArgumentNullException.ThrowIfNull(subjects);
        var lst = subjects.OrderByDescending(x => x.Id).ThenBy(x => x.Name);
        return lst.Select(s => new WrappedData()
        {
            GetId = () => s.Id.ToString(),
            GetString = () => s.Name,
        }).ToList();
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
    
    
    public static List<WrappedData> WrapQueueEvents(this List<QueueEventDto>? queueEvents)
    {
        ArgumentNullException.ThrowIfNull(queueEvents);

        var filtered = queueEvents
            .Where(q => q.EventDateTime < DateTime.Now - TimeSpan.FromDays(1))
            .OrderBy(q => q.EventDateTime);
        
        return filtered.Select(qEvent => new WrappedData()
            {
                GetId = () => qEvent.Id.ToString(),
                GetString = () => $"{qEvent.EventDateTime.ToShortDateString()} -- {qEvent.Name}",
            })
            .ToList();
    }
}