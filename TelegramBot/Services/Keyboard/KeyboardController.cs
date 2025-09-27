using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Models;
using static TelegramBot.Services.Keyboard.KeyboardGenerator;

namespace TelegramBot.Services.Keyboard;

public class KeyboardController(
    string callbackName,
    KeyboardSettings keyboardSettings)
{
    public InlineKeyboardMarkup? ActionKeyboardCallback(List<WrappedData> data, string callbackData)
    {
        var callback = CallbackDataView.LoadFromCallback(callbackData);

        if (callback.CallbackParam == "page")
        {
            if (callback.ExtraParam is null) throw new ArgumentException(nameof(callback.ExtraParam));
            
            keyboardSettings.PageNumber = int.Parse(callback.ExtraParam);
            return GenerateKeyboardPage(data, callbackName);
        }
        else
        {
            return null;
        }
        
    }

    public CallbackDataView GetCallbackDataView(string callbackData)
    {
        return CallbackDataView.LoadFromCallback(callbackData);
    }
    
    public InlineKeyboardMarkup GenerateKeyboard(List<WrappedData> data)
    {
        return GenerateKeyboardPage(data, callbackName);
    }
}