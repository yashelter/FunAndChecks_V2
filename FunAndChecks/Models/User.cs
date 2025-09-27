namespace FunAndChecks.Models;

using Microsoft.AspNetCore.Identity;

public class User : IdentityUser<Guid> // Используем Guid как тип ключа
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? GitHubUrl { get; set; }
    public long? TelegramUserId { get; set; }

    public int? GroupId { get; set; }
    public Group? Group { get; set; }
    
    public string? Color { get; set; }
    public string? Letter { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; }
    public ICollection<UserTaskSubmission> Submissions { get; set; } = new List<UserTaskSubmission>();
    public ICollection<QueueUser> QueueEntries { get; set; } = new List<QueueUser>();
}
