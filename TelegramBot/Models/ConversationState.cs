namespace TelegramBot.Models;

/// <summary>
/// Awaiting - в процессе прохождения, следующий ввод пользователя ожидается частью цепочки ввода
/// Pending  - мы ждём подтверждение корректности в callback, и игнорируем остальной ввод
/// </summary>
public enum ConversationState
{
    None,
    AwaitingRegister,
    PendingRegister,
    AwaitingLogin,
    PendingLogin,
}