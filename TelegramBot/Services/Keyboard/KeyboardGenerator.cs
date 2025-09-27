using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Models;

namespace TelegramBot.Services.Keyboard;

public static class KeyboardGenerator
{
    public class KeyboardSettings
    {
        public int PageNumber { get; set; } = 0;
        public int LineSize { get; set; } = 2;
        public int PageSize { get; set; } = 8;
    }

    /// <summary>
    /// Гененирирует страницу клавиатуры для списка
    /// </summary>
    /// <param name="data">Список обёрнутых значений</param>
    /// <param name="callbackData">Название клавиатуры для получения обратного результата</param>
    /// <param name="pageNumber">Номер страницы</param>
    /// <param name="lineSize"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static InlineKeyboardMarkup GenerateKeyboardPage(List<WrappedData> data, string callbackData, 
        int pageNumber = 0, int lineSize = 2, int pageSize = 8)
    {
        InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();
        
        List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
        for (int i = pageNumber * pageSize; i < Math.Min((pageNumber+1) * pageSize, data.Count); i++)
        {
            buttons.Add(InlineKeyboardButton.WithCallbackData(data[i].GetString(), 
                CallbackDataView.GenerateCallback(callbackData, data[i].GetId()).ToString()));
            if (buttons.Count == lineSize)
            {
                keyboard.AddNewRow(buttons.ToArray());
                buttons.Clear();
            }
        }
        keyboard.AddNewRow(buttons.ToArray());
        buttons.Clear();


        if (pageNumber > 0)
        {
            buttons.Add(InlineKeyboardButton.WithCallbackData("\u25c0\ufe0f", 
                CallbackDataView.GenerateCallback("page", callbackData, $"{pageNumber - 1}").ToString()));
        }

        //buttons.Add(InlineKeyboardButton.WithCallbackData("✖️", CallbackDataView.GenerateCallback("cancel_choosing", callbackData).ToString()));
        
        if ((pageNumber + 1) * pageSize < data.Count)
        {
            buttons.Add(InlineKeyboardButton.WithCallbackData("\u25b6\ufe0f", 
                CallbackDataView.GenerateCallback("page", callbackData, $"{pageNumber + 1}").ToString()));
        }

        keyboard.AddNewRow(buttons.ToArray());
        return keyboard;
    }
    
    
    /// <summary>
    /// Гененирирует страницу клавиатуры для списка
    /// </summary>
    /// <param name="data">Список обёрнутых значений</param>
    /// <param name="callbackData">Название клавиатуры для получения обратного результата</param>
    /// <param name="pageNumber">Номер страницы</param>
    /// <param name="lineSize"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static InlineKeyboardMarkup GenerateKeyboardPage(List<WrappedData> data, string callbackData, 
        KeyboardSettings keyboardSettings)
    {
       return GenerateKeyboardPage(data,
           callbackData,
           pageNumber: keyboardSettings.PageNumber,
           lineSize: keyboardSettings.LineSize,
           pageSize: keyboardSettings.PageSize);
    }
}