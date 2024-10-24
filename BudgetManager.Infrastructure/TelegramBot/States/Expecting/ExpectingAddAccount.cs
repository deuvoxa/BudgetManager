using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot.States.Expecting;

public class ExpectingAddAccount : UserStateBase
{
    public override async Task HandleAsync(ITelegramBotClient botClient, UserService userService, User user,
        long chatId, string messageText,
        CancellationToken cancellationToken)
    {
        var parameters = messageText.Split(":");

        // TODO: Проверка параметров
        
        var account = new Account
        {
            Name = parameters[0],
            Balance = decimal.Parse(parameters[1]),
            IsActive = true
        };
        
        user.Accounts.Add(account);
        await userService.UpdateAsync(user);
        
        var text = $"""
                    Добавил новый счёт `{parameters[0].ToLower()}`

                    Текущий баланс: {parameters[1]}
                    """;

        await botClient.EditMessageTextAsync(chatId, user.MainMessageId, text, replyMarkup: MainKeyboard.Back,
            parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);

        UserStates.State[chatId] = string.Empty;
    }
}