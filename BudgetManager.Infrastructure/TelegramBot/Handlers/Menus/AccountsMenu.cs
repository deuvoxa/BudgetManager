using BudgetManager.Application.Extensions;
using BudgetManager.Application.Services;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers.Menus;

public static class AccountsMenu
{
    public static async Task View(ITelegramBotClient botClient, CallbackQuery callbackQuery, UserService userService,
        CancellationToken cancellationToken)
    {
        var message = callbackQuery.Message!;
        var chatId = message.Chat.Id;

        var user = await userService.GetUserByTelegramIdAsync(chatId) ?? throw new Exception();
        
        var accounts = user.GetActiveAccount();

        var text = !accounts.Any()
            ? "У вас нет счетов, добавьте их"
            : $"{accounts.Aggregate("Твои счета:\n\n", (current, account) =>
                current + $"_{account.Name}_: `{account.Balance}` \u20bd\n\n")}";

        var keyboard = new KeyboardBuilder();
        if (accounts.Count >= 2)
            keyboard.WithButton("Перевод между счетами", "transfers-transfer");
        
        if (accounts.Any())
            keyboard.WithButton("Удалить счёт", "accounts-remove");

        keyboard.WithButton("Добавить счёт", "accounts-add")
            .WithButton("Вернуться назад", "main-menu");

        await botClient.EditMessageTextAsync(chatId: chatId, messageId: user.MainMessageId, text: text,
            replyMarkup: keyboard.Build(), parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
    }
}