using FunAndChecks.Models.JoinTables;

namespace FunAndChecks.Models;

public class Subject
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Связь "один ко многим" с Заданиями
    public ICollection<CourseTask> Tasks { get; set; } = new List<CourseTask>();

    // Связь "многие ко многим" с Группами
    public ICollection<GroupSubject> GroupSubjects { get; set; } = new List<GroupSubject>();
}