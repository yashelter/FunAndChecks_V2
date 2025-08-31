using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.Conversations;


public interface INotificationService
{
    Task EditMessageReplyMarkupAsync(long chatId, int messageId, InlineKeyboardMarkup? replyMarkup,
        ParseMode parseMode = ParseMode.Html);
    
    Task EditMessageTextAsync(long chatId, int messageId, string text, InlineKeyboardMarkup? replyMarkup = null,
        ParseMode parseMode = ParseMode.Html);
    
    Task<Message> SendTextMessageAsync(long chatId, string text, InlineKeyboardMarkup? replyMarkup = null, ParseMode parseMode = ParseMode.Html);
    
    Task SendConfirmationAsync(long chatId, string text, string yesCallback, string noCallback,
        string yesReply = "✅ Подтвердить", string noReply = "❌ Отмена");
    
}