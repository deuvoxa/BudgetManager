using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using BudgetManager.Infrastructure.TelegramBot.States;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers.Menus;

public static class TransactionsMenu
{
    public static async Task ExecuteAsync(
        ITelegramBotClient botClient, CallbackQuery callbackQuery,
        UserService userService, CancellationToken cancellationToken, int pageNumber = 1)
    {
        var message = callbackQuery.Message!;
        var chatId = message.Chat.Id;
        var user = await userService.GetUserByTelegramIdAsync(chatId);
        const int pageSize = 10;
        UserStates.State[chatId] = string.Empty;
        await userService.RemoveMetadata(chatId, "amount");
        await userService.RemoveMetadata(chatId, "accountId");
        await userService.RemoveMetadata(chatId, "isIncome");
        await userService.RemoveMetadata(chatId, "category");
        await userService.RemoveMetadata(chatId, "AmountTransfer");
        await userService.RemoveMetadata(chatId, "TargetAccountId");
        await userService.RemoveMetadata(chatId, "SourceAccountId");
        await userService.RemoveMetadata(chatId, "Liabilities");
        await userService.RemoveMetadata(chatId, "TransactionId");

        var transactions = user.Transactions.OrderByDescending(t => t.Date).ToList();
        var totalTransactions = transactions.Count;
        var totalPages = (int)Math.Ceiling((double)totalTransactions / pageSize);

        if (pageNumber > totalPages) pageNumber = totalPages;
        if (pageNumber < 1) pageNumber = 1;

        var paginatedTransactions = transactions
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t =>
                $"{t.Date:dd.MM.yyy} - {t.Category}: {(t.Type == TransactionType.Income ? "+" : "-")}{t.Amount} (`{t.Id.ToString()[..8]}`)")
            .ToList();

        var text = "*Все транзакции*\n\n" +
                   $"Страница {pageNumber}/{totalPages}\n\n" +
                   string.Join("\n", paginatedTransactions);

        var keyboard = new KeyboardBuilder();
        var navigationButtons = new List<(string, string)>();

        if (pageNumber > 1)
            navigationButtons.Add(("Назад", $"transactions-menu-{pageNumber - 1}"));
        if (pageNumber < totalPages)
            navigationButtons.Add(("Вперед", $"transactions-menu-{pageNumber + 1}"));

        keyboard.WithButtons(navigationButtons.ToArray());
        keyboard.WithButton("Добавить транзакцию", "transactions-add");
        keyboard.WithButton("Удалить транзакцию", "transactions-remove");
        keyboard.WithButton("Вернуться назад", "main-menu");

        await botClient.EditMessageTextAsync(chatId, message.MessageId, text, replyMarkup: keyboard.Build(),
            parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
    }
}