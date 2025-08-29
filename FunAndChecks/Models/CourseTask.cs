namespace FunAndChecks.Models;

public class CourseTask
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int MaxPoints { get; set; }

    // Связь "один ко многим" с Предметом
    public int SubjectId { get; set; }
    public Subject Subject { get; set; }

    // Навигационное свойство
    public ICollection<UserTaskSubmission> Submissions { get; set; } = new List<UserTaskSubmission>();
}
