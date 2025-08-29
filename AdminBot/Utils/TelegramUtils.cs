using Telegram.Bot.Types;

namespace AdminBot.Services.Utils;

public static class TelegramUtils
{
    public static long GetUserId(this Update update)
    {
        return update.Message?.From?.Id ?? 
               update.CallbackQuery?.From.Id ?? 
               update.InlineQuery?.From.Id ??
               update.ChosenInlineResult?.From.Id ?? 
               update.PreCheckoutQuery?.From.Id ?? 
               update.ShippingQuery?.From.Id ?? 
               throw new InvalidOperationException("Invalid try to get user id");
    }
    
    public static long GetChatId(this Update update)
    {
        return update.Message?.Chat.Id ?? 
               update.CallbackQuery?.From.Id ?? 
               throw new InvalidOperationException("Invalid try to get chat id");
    }
    
    public static int GetMessageId(this Update update)
    {
        return update.Message?.Id ?? 
               update.CallbackQuery?.Message?.Id ?? 
               throw new InvalidOperationException("Invalid try to get message id");
    }
    
    public static string GetMessageText(this Update update)
    {
        return update.Message?.Text ??
               update.CallbackQuery?.Message?.Text ??
               throw new InvalidOperationException("Invalid try to get message text");
    }
    
    public static string GetCallbackText(this Update update)
    {
        return update.CallbackQuery?.Data ??
               throw new InvalidOperationException("Invalid try to get message text");
    }
}