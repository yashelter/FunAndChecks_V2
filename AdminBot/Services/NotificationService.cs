using AdminBot.Conversations;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AdminBot.Services;

public class NotificationService(
    ITelegramBotClient bot, 
    ILogger<NotificationService> logger // TODO: log everything
    )
    : INotificationService
{
    public async Task EditMessageReplyMarkupAsync(long chatId, int messageId, InlineKeyboardMarkup replyMarkup,
        ParseMode parseMode = ParseMode.Html)
    {
        await bot.EditMessageReplyMarkup(chatId,
            messageId: messageId,
            replyMarkup: replyMarkup);
    }

    public async Task EditMessageTextAsync(long chatId, int messageId, string text, InlineKeyboardMarkup? replyMarkup,
        ParseMode parseMode = ParseMode.Html)
    {
        await bot.EditMessageText(chatId,
            messageId: messageId,
            text: text,
            replyMarkup: replyMarkup,
            parseMode: parseMode);
    }

    public async Task<Message> SendTextMessageAsync(long chatId, string text, InlineKeyboardMarkup? replyMarkup = null,  ParseMode parseMode = ParseMode.None)
    {
        return await bot.SendMessage(chatId,
            text,
            parseMode: parseMode,
            replyMarkup: replyMarkup);
    }

    public async Task SendConfirmationAsync(long chatId, string text, string yesCallback, string noCallback)
    {
        var inlineKeyboard = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData(text: "✅ Подтвердить", callbackData: yesCallback),
                InlineKeyboardButton.WithCallbackData(text: "❌ Отмена", callbackData: noCallback)
            ]
        ]);

        await bot.SendMessage(chatId,
            text,
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard);
    }
    
    
}