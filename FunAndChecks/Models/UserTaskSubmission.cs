using FunAndChecks.Models.Enums;

namespace FunAndChecks.Models;

// Эта таблица хранит историю всех оценок по заданиям
public class UserTaskSubmission
{
    public int Id { get; set; }
    public SubmissionStatus Status { get; set; }
    public string? Comment { get; set; }
    public DateTime SubmissionDate { get; set; }

    // Внешний ключ на пользователя, который сдал задание
    public Guid UserId { get; set; }
    public User User { get; set; }

    // Внешний ключ на само задание
    public int TaskId { get; set; }
    public CourseTask Task { get; set; }

    // Внешний ключ на админа, который проверил задание
    public Guid AdminId { get; set; }
    public User Admin { get; set; }
    
    // Необязательная привязка к событию в очереди
    public int? QueueEventId { get; set; }
    public QueueEvent? QueueEvent { get; set; }
}