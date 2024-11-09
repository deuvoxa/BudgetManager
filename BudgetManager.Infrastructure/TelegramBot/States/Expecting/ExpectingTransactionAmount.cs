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
        var category = user.Metadata.FirstOrDefault(m => m.Attribute is "category")?.Value;
        var accountId = user.Metadata.FirstOrDefault(m => m.Attribute is "accountId")?.Value;
        var account = user.GetActiveAccount().FirstOrDefault(a => a.Id == int.Parse(accountId));
        var isIncome = user.Metadata.FirstOrDefault(i => i.Attribute is "isIncome")?.Value is "income";

        if (!decimal.TryParse(messageText, out var amount) || amount <= 0)
        {
            await SendErrorAsync(botClient, chatId, "Ошибка: Введите корректную сумму транзакции больше нуля.",
                user, cancellationToken);
            return;
        }
        
        if (decimal.Parse(messageText) > account.Balance && !isIncome)
        {
            var warningText = "Сумма транзакции превышает баланс счёта!\n" +
                              $"Баланс счёта: `{account.Balance}`.\n\n" +
                              "Введите другую сумму, или выберите другой счёт.";
            
            await SendErrorAsync(botClient, chatId, warningText,
                user, cancellationToken);
            return;
        }
        
        var transaction = isIncome
            ? "доход"
            : "расход";

        var text = $"*Добавить {transaction}:*\n\n" +
                   $"*Категория:* `{category}`\n" +
                   $"*Счёт:* `{account.Name}`\n" +
                   $"*Сумма:* `{messageText}`\n" +
                   $"*Дата:* `{DateTime.UtcNow:dd.MM.yyy}`\n\n" +
                   $"Добавляю транзакцию?";

        var keyboard = new KeyboardBuilder()
            // TODO: Реализация кнопок
            .WithButton("Добавить описание", "d")
            .WithButton("Изменить дату", "a")
            .WithButtons([
                ("Да", "transactions-accept"),
                ("Нет", "transactions-menu")
            ]).Build();

        await userService.AddMetadata(chatId, "amount", messageText);

        await botClient.EditMessageTextAsync(chatId, user.MainMessageId, text, replyMarkup: keyboard,
            parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);

        UserStates.State[chatId] = string.Empty;
    }
    
    private async Task SendErrorAsync(ITelegramBotClient botClient, long chatId, string errorMessage, User user,
        CancellationToken cancellationToken)
    {
        var errorText = $"{errorMessage}\n\nПопробуйте снова.";

        try
        {
            await botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: user.MainMessageId,
                text: errorText,
                replyMarkup: MainKeyboard.Back,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            // ignored
        }
    }
}