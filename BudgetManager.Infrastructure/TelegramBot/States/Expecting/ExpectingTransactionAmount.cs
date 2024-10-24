using BudgetManager.Application.Extensions;
using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot.States.Expecting;

public class ExpectingTransactionAmount : UserStateBase
{
    public override async Task HandleAsync(ITelegramBotClient botClient, UserService userService, User user,
        long chatId, string messageText,
        CancellationToken cancellationToken)
    {
        // TODO: Проверка параметров

        var category = user.Metadata.FirstOrDefault(m => m.Attribute is "category")?.Value;
        var accountId = user.Metadata.FirstOrDefault(m => m.Attribute is "accountId")?.Value;
        var account = user.GetActiveAccount().FirstOrDefault(a => a.Id == int.Parse(accountId));
        var isIncome = user.Metadata.FirstOrDefault(i => i.Attribute is "isIncome")?.Value is "income";

        if (decimal.Parse(messageText) > account.Balance && !isIncome)
        {
            var warningText = "Сумма транзакции превышает баланс счёта!\n" +
                              $"Баланс счёта: `{account.Balance}`.\n\n" +
                              "Введите другую сумму, или выберите другой счёт.";

            try
            {
                await botClient.EditMessageTextAsync(
                    chatId, user.MainMessageId,
                    warningText,
                    replyMarkup: MainKeyboard.Back,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);
            }
            catch
            {
                // ignored
            }

            return;
        }
        
        var transaction = isIncome
            ? "доход"
            : "расход";

        var text = $"*Добавить {transaction}:*\n\n" +
                   $"*Категория:* `{category}`\n" +
                   $"*Счёт:* `{account.Name}`\n" +
                   $"*Сумма:* `{messageText}`\n\n" +
                   $"Добавляю транзакцию?";

        var keyboard = new KeyboardBuilder()
            .WithButtons([
                ("Да", "transactions-accept"),
                ("Нет", "main-menu")
            ]).Build();

        await userService.AddMetadata(chatId, "amount", messageText);

        await botClient.EditMessageTextAsync(chatId, user.MainMessageId, text, replyMarkup: keyboard,
            parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);

        UserStates.State[chatId] = string.Empty;
    }
}