using System.Text;
using BudgetManager.Application.Extensions;
using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using BudgetManager.Infrastructure.TelegramBot.States;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers.User;

public class TransactionHandler(
    ITelegramBotClient botClient,
    CallbackQuery callbackQuery,
    UserService userService,
    CancellationToken cancellationToken
) : HandlerBase(botClient, callbackQuery, cancellationToken)
{
    private readonly CallbackQuery _callbackQuery = callbackQuery;

    public async Task AddTransaction()
    {
        var text = "*Добавить транзакции:*\n\n" +
                   "Выберите категорию:";

        List<(string, string)> categories =
        [
            ("Продукты", "transactions-selectCategory-food"),
            ("Транспорт", "transactions-selectCategory-transport"),
            ("Зарплата", "transactions-selectCategory-salary"),
            ("Другое", "transactions-selectCategory-other"),
            ("Жильё", "transactions-selectCategory-house")
        ];

        var user = await userService.GetUserByTelegramIdAsync(_callbackQuery.Message!.Chat.Id);

        categories.AddRange(user.Metadata.Where(m => m.Attribute is "Category")
            .Select(category => (category.Value, $"category-selectCategory-{category.Value}")));

        var keyboard = new KeyboardBuilder()
            .WithButtonGrid(categories)
            .WithBackToTransactions()
            .Build();

        await EditMessage(text, keyboard);
    }

    public async Task SelectCategory(string category)
    {
        var user = await userService.GetUserByTelegramIdAsync(_callbackQuery.Message!.Chat.Id);

        category = category switch
        {
            "food" => "Продукты",
            "transport" => "Транспорт",
            "salary" => "Зарплата",
            "house" => "Жильё",
            "other" => "Другое",
            _ => category
        };


        await userService.AddMetadata(user.TelegramId, "category", category);

        var accounts = user.GetActiveAccount();

        var tuples = accounts.Select(account => ($"{account.Name}", $"transactions-selectAccount-{account.Id}"))
            .ToArray();

        var keyboard = accounts.Count == 0
            ? MainKeyboard.Back
            : new KeyboardBuilder()
                .WithButtonGrid(tuples)
                .WithButton("Вернуться назад", "main-menu")
                .Build();

        var text = accounts.Count == 0
            ? "Для добавления новых транзакций, необходимо добавить счёт."
            : "*Добавить транзакцию:*\n\n" +
              $"*Категория:* `{category}`\n\n" +
              "Выбери счёт:";

        await EditMessage(text, keyboard);
    }

    public async Task SelectAccount(string accountId)
    {
        var message = _callbackQuery.Message!;
        var chatId = message.Chat.Id;
        var user = await userService.GetUserByTelegramIdAsync(chatId);

        var account = user.GetActiveAccount().FirstOrDefault(a => a.Id == int.Parse(accountId));

        await userService.AddMetadata(chatId, "accountId", accountId);

        var category = user.Metadata.FirstOrDefault(c => c.Attribute == "category")?.Value;

        var text = "*Добавить транзакцию:*\n\n" +
                   $"*Категория:* `{category}`\n" +
                   $"*Счёт:* `{account.Name}`\n\n" +
                   "Это доход, или расход?";

        var keyboard = new KeyboardBuilder()
            .WithButtons([
                ("Доход", "transactions-select-income"),
                ("Расход", "transactions-select-expense"),
            ])
            .WithButton("Вернуться назад", "main-menu")
            .Build();

        await EditMessage(text, keyboard);
    }

    public async Task SelectIncomeExpense(string isIncome)
    {
        var chatId = _callbackQuery.Message!.Chat.Id;
        var user = await userService.GetUserByTelegramIdAsync(chatId);
        await userService.AddMetadata(user.TelegramId, "isIncome", isIncome);

        var category = user.Metadata.FirstOrDefault(m => m.Attribute is "category")?.Value;
        var accountId = user.Metadata.FirstOrDefault(m => m.Attribute is "accountId")?.Value;
        var account = user.GetActiveAccount().FirstOrDefault(a => a.Id == int.Parse(accountId));
        var incomeOrExpense = isIncome is "income" ? "доход" : "расход";

        var text = $"*Добавить {incomeOrExpense}:*\n\n" +
                   $"*Категория:* `{category}`\n" +
                   $"*Счёт:* `{account.Name}`\n\n" +
                   "Введи сумму транзакции:";

        await EditMessage(text, MainKeyboard.Back);

        UserStates.State[chatId] = "ExpectingAmountTransaction";
    }

    public async Task AcceptTransaction()
    {
        var message = _callbackQuery.Message!;
        var chatId = message.Chat.Id;
        var user = await userService.GetUserByTelegramIdAsync(chatId);

        var category = user.Metadata.First(m => m.Attribute is "category").Value;
        var accountId = user.Metadata.First(m => m.Attribute is "accountId").Value;
        var account = user.GetActiveAccount().First(a => a.Id == int.Parse(accountId));
        var isIncome = user.Metadata.First(m => m.Attribute is "isIncome").Value == "income";
        var amount = user.Metadata.First(m => m.Attribute is "amount").Value;

        var transactionType = isIncome
            ? TransactionType.Income
            : TransactionType.Expense;

        var transaction = new Transaction
        {
            User = user,
            Account = account,
            Amount = decimal.Parse(amount),
            Category = category,
            Date = DateTime.UtcNow,
            // TODO: Добавить реализацию добавления заметок к транзакциям
            Description = "",
            Type = transactionType,
        };

        user.Transactions.Add(transaction);

        if (isIncome) account.Balance += transaction.Amount;
        else account.Balance -= transaction.Amount;

        await userService.UpdateAsync(user);

        await EditMessage("Транзакция успешно добавлена!", new KeyboardBuilder().WithBackToTransactions().Build());
    }

    public async Task DeleteTransaction()
    {
        var chatId = _callbackQuery.Message!.Chat.Id;
        var user = await userService.GetUserByTelegramIdAsync(chatId);

        if (!user.Metadata.Any(m => m.Attribute is "TransactionId"))
        {
            UserStates.State[chatId] = "ExpectingShortTransactionId";

            await EditMessage("Введите ID транзакции. Найти его можно в меню транзакций в скобках вида (35f02290)",
                new KeyboardBuilder().WithBackToTransactions().Build());
            return;
        }

        var transactionId = user.Metadata.FirstOrDefault(m => m.Attribute is "TransactionId")?.Value;

        var transaction = user.Transactions.FirstOrDefault(t => t.Id.ToString().StartsWith(transactionId)
        );

        if (transaction != null)
        {
            user.Transactions.Remove(transaction);
            await userService.UpdateAsync(user);

            await EditMessage("Транзакция успешно удалена!", new KeyboardBuilder().WithBackToTransactions().Build());
        }
        else
        {
            await EditMessage("Транзакция с таким ID не найдена.",
                new KeyboardBuilder().WithBackToTransactions().Build());
        }
    }
}