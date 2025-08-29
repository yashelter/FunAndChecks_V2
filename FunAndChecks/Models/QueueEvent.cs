using FunAndChecks.Models.JoinTables;

namespace FunAndChecks.Models;

public class QueueEvent
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime EventDateTime { get; set; }

    // Связь с Предметом
    public int SubjectId { get; set; }
    public Subject Subject { get; set; }


    // Пользователи, записанные в эту очередь
    public ICollection<QueueUser> Participants { get; set; } = new List<QueueUser>();
}