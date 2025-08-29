using FunAndChecks.Models.JoinTables;

namespace FunAndChecks.Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    // M8О-XYY-ZZ
    public int StartYear { get; set; }    // ZZ
    public int GroupNumber { get; set; }  // YY

    // Связь "один ко многим" с Пользователями
    public ICollection<User> Users { get; set; } = new List<User>();

    // Связь "многие ко многим" с Предметами
    public ICollection<GroupSubject> GroupSubjects { get; set; } = new List<GroupSubject>();
}