using FunAndChecks.Models;
using FunAndChecks.Models.JoinTables;

namespace FunAndChecks.Data;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Регистрируем все наши таблицы
    public DbSet<Group> Groups { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<CourseTask> Tasks { get; set; }
    public DbSet<UserTaskSubmission> UserTaskSubmissions { get; set; }
    public DbSet<QueueEvent> QueueEvents { get; set; }
    public DbSet<QueueUser> QueueUsers { get; set; }

    // Соединительные таблицы
    public DbSet<GroupSubject> GroupSubjects { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Обязательно для Identity
       
        builder.Entity<User>(b =>
        {
            // У каждого пользователя есть много записей UserRole
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.User) // Каждая запись UserRole связана с одним пользователем
                .HasForeignKey(ur => ur.UserId) // Связь осуществляется через внешний ключ UserId
                .IsRequired();
        });

        // Настраиваем связь со стороны Role
        builder.Entity<Role>(b =>
        {
            // У каждой роли есть много записей UserRole
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role) // Каждая запись UserRole связана с одной ролью
                .HasForeignKey(ur => ur.RoleId) // Связь осуществляется через внешний ключ RoleId
                .IsRequired();
        });
        
        builder.Entity<Group>()
            .HasMany(g => g.Users)
            .WithOne(u => u.Group)
            .HasForeignKey(u => u.GroupId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.Entity<Subject>()
            .HasMany(s => s.Tasks)
            .WithOne(t => t.Subject)
            .HasForeignKey(t => t.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<CourseTask>()
            .HasMany(t => t.Submissions)
            .WithOne(s => s.Task)
            .HasForeignKey(s => s.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Настройка составного первичного ключа для GroupSubject
        builder.Entity<GroupSubject>()
            .HasKey(gs => new { gs.GroupId, gs.SubjectId });

        // Настройка связи Group -> GroupSubject -> Subject
        builder.Entity<GroupSubject>()
            .HasOne(gs => gs.Group)
            .WithMany(g => g.GroupSubjects)
            .HasForeignKey(gs => gs.GroupId);

        builder.Entity<GroupSubject>()
            .HasOne(gs => gs.Subject)
            .WithMany(s => s.GroupSubjects)
            .HasForeignKey(gs => gs.SubjectId);
        

        // Настройка связи для Admin в UserTaskSubmission
        builder.Entity<UserTaskSubmission>()
            .HasOne(s => s.Admin)
            .WithMany() // У админа много проверок, но обратная навигация не нужна
            .HasForeignKey(s => s.AdminId)
            .OnDelete(DeleteBehavior.Restrict); // Запрещаем удаление админа, если у него есть проверки

        // Настройка связи User -> UserTaskSubmission
        builder.Entity<UserTaskSubmission>()
            .HasOne(s => s.User)
            .WithMany(u => u.Submissions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade); // При удалении пользователя удаляем его сдачи
        builder.Entity<QueueUser>()
            .HasOne(qu => qu.CurrentAdmin) // У записи QueueUser есть один CurrentAdmin...
            .WithMany() // ...а у User (в роли админа) может быть много записей в очереди, которые он проверяет. Обратная навигация (коллекция) в классе User нам не нужна.
            .HasForeignKey(qu => qu.CurrentAdminId) // Явно указываем, что внешний ключ для этой связи — это поле CurrentAdminId.
            .OnDelete(DeleteBehavior.SetNull); // Очень важно! Указываем, что если админ будет удален, то поле CurrentAdminId в этой таблице должно стать NULL, а не вызывать ошибку или удалять запись из очереди.
    }
}