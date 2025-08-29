namespace FunAndChecks.Models.JoinTables;

public class GroupSubject
{
    public int GroupId { get; set; }
    public Group Group { get; set; }

    public int SubjectId { get; set; }
    public Subject Subject { get; set; }
}