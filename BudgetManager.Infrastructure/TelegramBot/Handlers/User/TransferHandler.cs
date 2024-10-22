using BudgetManager.Application.Services;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using BudgetManager.Infrastructure.TelegramBot.States;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers.User;

public class TransferHandler(
    ITelegramBotClient botClient,
    CallbackQuery callbackQuery,
    Domain.Entities.User user,
    UserService userService,
    CancellationToken cancellationToken)
{
    public async Task ChooseSourceAccount()
    {
        var accounts = user.Accounts;

        var tuples = accounts.Select(account => ($"{account.Name}", $"source-{account.Id}"))
            .ToArray();

        var keyboard = accounts.Count == 0
            ? MainKeyboard.Back
            : new KeyboardBuilder()
                .WithButtonGrid(tuples)
                .WithButton("Вернуться назад", "main-menu")
                .Build();

        await EditMessage("Выберите счёт для списания:", keyboard);
    }

    public async Task ChooseTargetAccount(int sourceAccountId)
    {
        var accounts = user.Accounts.Where(a => a.Id != sourceAccountId); // Исключаем исходный счет
        var tuples = accounts.Select(account => ($"{account.Name}", $"target-{account.Id}"))
            .ToArray();

        var sourceAccount = user.Accounts.FirstOrDefault(x => x.Id == sourceAccountId);

        await userService.AddMetadata(user.TelegramId, "SourceAccountId", sourceAccountId.ToString());

        var keyboard = !accounts.Any()
            ? MainKeyboard.Back
            : new KeyboardBuilder()
                .WithButtonGrid(tuples)
                .WithButton("Вернуться назад", "main-menu")
                .Build();

        var text = $"{sourceAccount.Name}:\nБаланс: {sourceAccount.Balance}\n\n" +
                   $"Выберите счёт для зачисления:";

        await EditMessage(text, keyboard);
    }

    public async Task EnterTransferAmount(int targetAccountId)
    {
        await userService.AddMetadata(user.TelegramId, "TargetAccountId", targetAccountId.ToString());

        var sourceAccount = user.Accounts.FirstOrDefault(x =>
            x.Id == int.Parse(user.Metadata.FirstOrDefault(x => x.Attribute == "SourceAccountId").Value));

        var targetAccount = user.Accounts.FirstOrDefault(x => x.Id == targetAccountId);

        var text = $"Перевожу со счёта *{sourceAccount.Name}*:\n*Баланс*: {sourceAccount.Balance}\n\n" +
                   $"на счёт *{targetAccount.Name}*\n*Баланс*: {targetAccount.Balance}\n\n" +
                   $"Введите сумму для перевода:";

        var chatId = callbackQuery.Message!.Chat.Id;
        UserStates.State[chatId] = "ExpectingTransferAmount";

        await EditMessage(text, MainKeyboard.Back);
    }

    public async Task AcceptTransfer()
    {
        var sourceAccount = user.Accounts.First(x =>
            x.Id == int.Parse(user.Metadata.First(x => x.Attribute == "SourceAccountId").Value));

        var targetAccount = user.Accounts.First(x =>
            x.Id == int.Parse(user.Metadata.First(x => x.Attribute == "TargetAccountId").Value));

        var amount = decimal.Parse(user.Metadata.First(m => m.Attribute == "AmountTransfer").Value);

        if (sourceAccount.Balance < amount)
        {
            await EditMessage("Ошибка: недостаточно средств.", MainKeyboard.Back);
            return;
        }

        sourceAccount.Balance -= amount;
        targetAccount.Balance += amount;

        await userService.UpdateAsync(user);

        await EditMessage(
            $"Перевод успешно выполнен. {amount} переведено с {sourceAccount.Name} на {targetAccount.Name}.",
            MainKeyboard.Back);
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