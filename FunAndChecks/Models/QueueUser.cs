using FunAndChecks.Models.Enums;

namespace FunAndChecks.Models;

public class QueueUser
{
    public int Id { get; set; }
    public DateTime JoinTime { get; set; }
    public QueueUserStatus Status { get; set; }

    // Связь с событием очереди
    public int QueueEventId { get; set; }
    public QueueEvent QueueEvent { get; set; }

    // Связь с пользователем
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public Guid? CurrentAdminId { get; set; }
    public User? CurrentAdmin { get; set; }
}