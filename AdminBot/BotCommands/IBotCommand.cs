using Telegram.Bot.Types;

namespace AdminBot.BotCommands;

public interface  IBotCommand
{
    // Текстовая команда, на которую реагирует класс (например, "/create_new_subject")

    string Name { get; }
    
    // Метод для выполнения команды
    Task ExecuteAsync(Update update);
}