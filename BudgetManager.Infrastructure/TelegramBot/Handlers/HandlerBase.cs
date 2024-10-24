using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers;

public abstract class HandlerBase(
    ITelegramBotClient botClient,
    CallbackQuery callbackQuery,
    CancellationToken cancellationToken)
{
    protected async Task EditMessage(string text, InlineKeyboardMarkup keyboard)
    {
        await botClient.EditMessageTextAsync(
            callbackQuery.Message!.Chat.Id,
            callbackQuery.Message.MessageId,
            text,
            replyMarkup: keyboard,
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
    }
}