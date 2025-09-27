using Telegram.Bot.Types;

namespace TelegramBot.BotCommands;

public interface  IBotCommand
{
    /// <summary>
    /// Текстовая команда, на которую реагирует класс (например, "/create_new_subject")
    /// </summary>
    string Name { get; }
    
    
    /// <summary>
    /// Метод для выполнения команды
    /// </summary>
    /// <param name="update">Обновление из Telegram API</param>
    /// <returns></returns>
    Task ExecuteAsync(Update update);
}