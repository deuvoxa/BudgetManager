using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers.User;

public static class ViewAccounts
{
    public static async Task Execute(ITelegramBotClient botClient, CallbackQuery callbackQuery, UserService
        userService, CancellationToken cancellationToken)
    {
        var message = callbackQuery.Message!;
        var chatId = message.Chat.Id;

        var user = await userService.GetUserByTelegramIdAsync(chatId);

        var accounts = user.Accounts;

        var text = accounts.Count == 0
            ? "У вас нет счетов, добавьте их"
            : $"{accounts.Aggregate("Твои счета:\n\n", (current, account) =>
                current + $"{account.Name} - {account.Balance} руб\n\n")}";

        var keyboard = new KeyboardBuilder()
            .WithButton("Перевод между счетами", "transfer")
            .WithButton("Добавить новый счёт", "add-account")
            .WithButton("Вернуться назад", "main-menu")
            .Build();

        await botClient.EditMessageTextAsync(chatId: chatId, messageId: user.MainMessageId, text: text,
            replyMarkup: keyboard, cancellationToken: cancellationToken);
    }
}