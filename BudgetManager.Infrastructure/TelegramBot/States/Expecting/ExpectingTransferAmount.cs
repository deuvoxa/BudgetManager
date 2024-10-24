using BudgetManager.Application.Extensions;
using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot.States.Expecting;

public class ExpectingTransferAmount : UserStateBase
{
    public override async Task HandleAsync(ITelegramBotClient botClient, UserService userService, User user,
        long chatId, string messageText, CancellationToken cancellationToken)
    {
        var sourceAccount = user.GetActiveAccount().FirstOrDefault(x =>
            x.Id == int.Parse(user.Metadata.FirstOrDefault(x => x.Attribute == "SourceAccountId").Value));
        
        var targetAccount = user.GetActiveAccount().FirstOrDefault(x =>
            x.Id == int.Parse(user.Metadata.FirstOrDefault(x => x.Attribute == "TargetAccountId").Value));

        // TODO: Проверка значений от пользователя
        var text = $"""
                   Перевожу со счёта *{sourceAccount.Name}*
                   на счёт *{targetAccount.Name}* `{messageText}`?
                   """;
        
        var keyboard = new KeyboardBuilder()
            .WithButtons([
                ("Да", "transfers-accept"),
                ("Нет", "main-menu")
            ]).Build();

        await userService.AddMetadata(chatId, "AmountTransfer", messageText);

        await botClient.EditMessageTextAsync(chatId, user.MainMessageId, text, replyMarkup: keyboard,
            parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);

        UserStates.State[chatId] = string.Empty;
    }
}