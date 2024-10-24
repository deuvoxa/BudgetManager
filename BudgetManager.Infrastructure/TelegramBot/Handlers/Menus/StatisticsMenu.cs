using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers.Menus;

public static class StatisticsMenu
{
    public static async Task ExecuteAsync(
        ITelegramBotClient botClient, CallbackQuery callbackQuery,
        UserService userService, CancellationToken cancellationToken)
    {
        var message = callbackQuery.Message!;
        var chatId = message.Chat.Id;

        var user = await userService.GetUserByTelegramIdAsync(chatId);

        var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var endOfToday = DateTime.Now;

        var totalIncome = user.Transactions
            .Where(t => t.Date >= startOfMonth && t.Date <= endOfToday && t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpenses = user.Transactions
            .Where(t => t.Date >= startOfMonth && t.Date <= endOfToday && t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var text = $"""
                    *Меню статистики:*

                    Доход за текущий месяц: {totalIncome} ₽
                    Расход за текущий месяц: {totalExpenses} ₽
                    """;

        var buttons = new List<(string, string)>();
        
        if (totalIncome > 0) buttons.Add(("Доходы", "statistics-get-incomes"));
        if (totalExpenses > 0) buttons.Add(("Расходы", "statistics-get-expenses"));
        
        var keyboard = new KeyboardBuilder()
            .WithButtons(buttons)
            .WithBackToHome();

        await botClient.EditMessageTextAsync(chatId, message.MessageId, text, replyMarkup: keyboard.Build(),
            parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
    }
}