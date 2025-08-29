namespace FunAndChecks.Models.Enums;

/// <summary>
/// Порядок влияет!
/// </summary>
public enum QueueUserStatus
{
    Checking = 0,   // Проверяется
    Waiting = 1,    // Ожидает
    Finished = 2,   // Закончил
    Skipped = 3     // Пропущен
}