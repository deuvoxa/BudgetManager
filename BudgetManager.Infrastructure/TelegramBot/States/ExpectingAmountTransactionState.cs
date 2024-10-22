using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetManager.Infrastructure.TelegramBot.States;

public class ExpectingAmountTransactionState : UserStateBase
{
    public override async Task HandleAsync(ITelegramBotClient botClient, UserService userService, User user,
        long chatId, string messageText,
        CancellationToken cancellationToken)
    {
        // TODO: Проверка параметров


        var category = user.Metadata.FirstOrDefault(m => m.Attribute is "category")?.Value;
        var accountId = user.Metadata.FirstOrDefault(m => m.Attribute is "accountId")?.Value;
        var account = user.Accounts.FirstOrDefault(a => a.Id == int.Parse(accountId));
        var isIncome = user.Metadata.FirstOrDefault(i => i.Attribute is "isIncome")?.Value is "income"
            ? "доход"
            : "расход";

        var text = $"*Добавить {isIncome}:*\n\n" +
                   $"*Категория:* `{category}`\n" +
                   $"*Счёт:* `{account.Name}`\n" +
                   $"*Сумма:* `{messageText}`\n\n" +
                   $"Добавляю транзакцию?";

        var keyboard = new KeyboardBuilder()
            .WithButtons([
                ("Да", "accept-transaction"),
                ("Нет", "main-menu")
            ]).Build();

        await userService.AddMetadata(chatId, "amount", messageText);

        await botClient.EditMessageTextAsync(chatId, user.MainMessageId, text, replyMarkup: keyboard,
            parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);

        UserStates.State[chatId] = string.Empty;
    }
}