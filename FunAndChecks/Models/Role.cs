namespace FunAndChecks.Models;

using Microsoft.AspNetCore.Identity;

public class Role : IdentityRole<Guid>
{
    public virtual ICollection<UserRole> UserRoles { get; set; }

}
