namespace FunAndChecks.Models.Enums;

/// <summary>
/// Порядок влияет!
/// </summary>
public enum QueueUserStatus
{
    Checking = 0,   // Проверяется
    Waiting = 1,    // Ожидает
    Skipped = 2,     // Пропущен
    Finished = 3,   // Закончил
}