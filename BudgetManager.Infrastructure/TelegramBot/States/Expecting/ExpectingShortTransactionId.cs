using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot.States.Expecting;

public class ExpectingShortTransactionId : UserStateBase
{
    public override async Task HandleAsync(ITelegramBotClient botClient, UserService userService, User user,
        long chatId, string messageText, CancellationToken cancellationToken)
    {
        if (messageText.Length != 8 || !messageText.All(char.IsLetterOrDigit))
        {
            await SendErrorAsync(botClient, chatId,
                "Некорректный формат ID. Пожалуйста, введите 8 символов, соответствующих ID транзакции.", user,
                cancellationToken);
            return;
        }

        var transaction = user.Transactions
            .FirstOrDefault(t => t.Id.ToString("N").StartsWith(messageText, StringComparison.OrdinalIgnoreCase));

        if (transaction is null)
        {
            await SendErrorAsync(botClient, chatId,
                "Транзакция с указанным ID не найдена. Пожалуйста, проверьте ID и попробуйте снова.", user,
                cancellationToken);
            return;
        }

        await userService.AddMetadata(chatId, "TransactionId", transaction.Id.ToString());
        
        // TODO: Решить, учитывать ли баланс счетов при удалении, или нет
        // TODO: Вывод самой транзакции
        // TODO: Уточнять перед удалением
        
        var account = user.Accounts.FirstOrDefault(a => a.Id == transaction.AccountId);
        // if (transaction.Type == TransactionType.Income)
        //     account.Balance -= transaction.Amount;
        // else
        //     account.Balance += transaction.Amount;

        var text = $"Удаляю транзакцию с ID `{messageText}`?\n\n" +
                   $"*Счёт:* `{account.Name}`\n" +
                   $"*Тип:* {(transaction.Type == TransactionType.Income ? "Доход" : "Расход")}\n" +
                   $"*Сумма:* `{transaction.Amount}`\n" +
                   $"*Дата:* `{transaction.Date:dd.MM.yyy}`";

        var keyboard = new KeyboardBuilder()
            .WithButtons([
                ("Да", "transactions-remove"),
                ("Нет", "transactions-menu")
            ]).Build();

        await botClient.EditMessageTextAsync(chatId, user.MainMessageId, text, replyMarkup: keyboard,
            parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
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
        catch (Exception)
        {
            // 
        }
    }
}