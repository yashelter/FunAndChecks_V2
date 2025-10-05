using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Conversations;

namespace TelegramBot.Services;

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

    public async Task DeleteMessageAsync(long chatId, int messageId)
    {
        await bot.DeleteMessage(chatId, messageId);
    }

    public async Task SendConfirmationAsync(long chatId, string text, string yesCallback, string noCallback,
        string yesReply = "✅ Подтвердить", string noReply = "❌ Отмена")
    {
        var inlineKeyboard = new InlineKeyboardMarkup([
            [
                InlineKeyboardButton.WithCallbackData(text: yesReply, callbackData: yesCallback),
                InlineKeyboardButton.WithCallbackData(text: noReply, callbackData: noCallback)
            ]
        ]);
        
        await bot.SendMessage(chatId,
            text,
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard);
    }

    public async Task SendJoinQueueMenuAsync(long chatId)
    {
        var replyKeyboardMarkup = new ReplyKeyboardMarkup([["/join_queue"]])
        {
            OneTimeKeyboard = false 
        };

        await bot.SendMessage(
            chatId: chatId,
            text: "Выберите команду из меню:",
            replyMarkup: replyKeyboardMarkup
        );
    }
}