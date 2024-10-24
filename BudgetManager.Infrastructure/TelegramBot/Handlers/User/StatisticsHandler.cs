using BudgetManager.Application.Extensions;
using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers.User;

public class StatisticsHandler(
    ITelegramBotClient botClient,
    CallbackQuery callbackQuery,
    UserService userService,
    CancellationToken cancellationToken
) : HandlerBase(botClient, callbackQuery, cancellationToken)
{
    private readonly CallbackQuery _callbackQuery = callbackQuery;

    public async Task GetStatistics(string transaction, string period)
    {
        var transactionType = transaction == "incomes" ? TransactionType.Income : TransactionType.Expense;
        var user = await userService.GetUserByTelegramIdAsync(_callbackQuery.From.Id)
                   ?? throw new NullReferenceException();

        var transactions = period switch
        {
            "day" => user.GetTransactionsForCurrentDay(),
            "week" => user.GetTransactionsForCurrentWeek(),
            _ => user.GetTransactionsForCurrentMonth()
        };

        var dictionary = transactions
            .Where(t => t.Type == transactionType)
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        var text = $"{(transaction == "incomes" ? "_Доход за_" : "_Расход за_")} {GetPeriodText(period)}:\n\n" +
                   string.Join("\n", dictionary.Select(stat => $"{stat.Key}: {stat.Value}"));

        var buttons = period switch
        {
            "day" =>
            [
                ("За неделю", $"statistics-get-{transaction}-week"),
                ("За месяц", $"statistics-get-{transaction}-month")
            ],
            "week" =>
            [
                ("За сегодня", $"statistics-get-{transaction}-day"),
                ("За месяц", $"statistics-get-{transaction}-month")
            ],
            _ => new[]
            {
                ("За сегодня", $"statistics-get-{transaction}-day"),
                ("За неделю", $"statistics-get-{transaction}-week")
            }
        };

        var keyboard = new KeyboardBuilder().WithButtons(buttons)
            .WithButton("Вернуться назад", "statistics-menu")
            .Build();
        await EditMessage(text, keyboard);
    }

    private static string GetPeriodText(string period) => period switch
    {
        "day" => "сегодня",
        "week" => "неделю",
        _ => "месяц"
    };
}