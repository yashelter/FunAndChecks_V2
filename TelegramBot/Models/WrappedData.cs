namespace TelegramBot.Models;

/// <summary>
/// Упаковывает любые типы данных для клавиатуры.
/// Id нужен для использования после нажатия на клавиатуру - формально что угодно
/// Строка - то что видит пользователь.
/// </summary>
public class WrappedData
{
    public required Func<string> GetId;
    public required Func<string> GetString;
}