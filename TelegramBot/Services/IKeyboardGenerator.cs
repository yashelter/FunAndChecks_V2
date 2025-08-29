using TelegramBot.Models;

namespace TelegramBot.Services;

using Telegram.Bot.Types.ReplyMarkups;


public interface IKeyboardGenerator
{
    public InlineKeyboardMarkup GenerateKeyboardPage(List<WrappedData> data, string callbackData,
        int pageNumber = 0, int lineSize = 2, int pageSize = 6);
}