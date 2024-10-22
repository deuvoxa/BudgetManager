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
    CancellationToken cancellationToken)
{
    public async Task AddTransaction()
    {
        var text = "*Добавить транзакции:*\n\n" +
                   "Выберите категорию:";

        // TODO: Добавление собственных категорий через бота
        (string, string)[] categories =
        [
            ("Продукты", "category-food"),
            ("Транспорт", "category-transport"),
            ("Зарплата", "category-salary"),
            ("Другое", "category-other"),
            ("Жильё", "category-house")
        ];

        var keyboard = new KeyboardBuilder()
            .WithButtonGrid(categories)
            .WithButton("Вернуться назад", "main-menu")
            .Build();

        await EditMessage(text, keyboard);
    }

    public async Task SelectCategory(string category)
    {
        var user = await userService.GetUserByTelegramIdAsync(callbackQuery.Message!.Chat.Id);

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

        var accounts = user.Accounts;

        var tuples = accounts.Select(account => ($"{account.Name}", $"account-{account.Id}")).ToArray();

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
        var message = callbackQuery.Message!;
        var chatId = message.Chat.Id;
        var user = await userService.GetUserByTelegramIdAsync(chatId);

        var account = user.Accounts.FirstOrDefault(a => a.Id == int.Parse(accountId));

        await userService.AddMetadata(chatId, "accountId", accountId);

        var category = user.Metadata.FirstOrDefault(c => c.Attribute == "category")?.Value;

        var text = "*Добавить транзакцию:*\n\n" +
                   $"*Категория:* `{category}`\n" +
                   $"*Счёт:* `{account.Name}`\n\n" +
                   "Это доход, или расход?";

        var keyboard = new KeyboardBuilder()
            .WithButtons([
                ("Доход", "select-income"),
                ("Расход", "select-expense"),
            ])
            .WithButton("Вернуться назад", "main-menu")
            .Build();

        await EditMessage(text, keyboard);
    }

    public async Task SelectIncomeExpense(string isIncome)
    {
        var chatId = callbackQuery.Message!.Chat.Id;
        var user = await userService.GetUserByTelegramIdAsync(chatId);
        await userService.AddMetadata(user.TelegramId, "isIncome", isIncome);

        var category = user.Metadata.FirstOrDefault(m => m.Attribute is "category")?.Value;
        var accountId = user.Metadata.FirstOrDefault(m => m.Attribute is "accountId")?.Value;
        var account = user.Accounts.FirstOrDefault(a => a.Id == int.Parse(accountId));
        var incomeOrExpense = isIncome is "income" ? "доход" : "расход";

        var text = $"*Добавить {incomeOrExpense}:*\n\n" +
                   $"*Категория:* `{category}`\n" +
                   $"*Счёт:* `{account.Name}`\n\n" +
                   "Введи сумму транзакции:";

        await EditMessage(text, MainKeyboard.Back);

        UserStates.State[chatId] = "ExpectingAmountTransactionState";
    }

    public async Task AcceptTransaction()
    {
        var message = callbackQuery.Message!;
        var chatId = message.Chat.Id;
        var user = await userService.GetUserByTelegramIdAsync(chatId);

        var category = user.Metadata.First(m => m.Attribute is "category").Value;
        var accountId = user.Metadata.First(m => m.Attribute is "accountId").Value;
        var account = user.Accounts.First(a => a.Id == int.Parse(accountId));
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

        await EditMessage("Транзакция успешно добавлена!", MainKeyboard.Back);
    }
    
    private async Task EditMessage(string text, InlineKeyboardMarkup keyboard)
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