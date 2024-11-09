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

        if (parameters.Length < 2)
        {
            await SendErrorAsync(botClient, chatId,
                "Неверный формат данных. Пожалуйста, используйте формат: `название`:`баланс`",
                user, cancellationToken);
            return;
        }

        if (!decimal.TryParse(parameters[1], out var amount) || amount <= 0)
        {
            await SendErrorAsync(botClient, chatId,
                "Неверно указана сумма. Пожалуйста, введите корректное число.",
                user, cancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(parameters[0]))
        {
            await SendErrorAsync(botClient, chatId, "Название счёта не может быть пустым. Укажите корректное название.",
                user, cancellationToken);
            return;
        }


        var account = new Account
        {
            Name = parameters[0],
            Balance = amount,
            IsActive = true
        };

        user.Accounts.Add(account);
        await userService.UpdateAsync(user);

        var text = $"""
                    Добавил новый счёт `{parameters[0].ToLower()}`

                    Текущий баланс: {parameters[1]}
                    """;

        var keyboard = new KeyboardBuilder()
            .WithButton("Вернуться назад", "accounts-menu")
            .Build();
        
        await botClient.EditMessageTextAsync(
            chatId: chatId,
            messageId: user.MainMessageId,
            text: text,
            replyMarkup: keyboard,
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);

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