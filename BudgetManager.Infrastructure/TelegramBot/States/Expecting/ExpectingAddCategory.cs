using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot.States.Expecting;

public class ExpectingAddCategory : UserStateBase
{
    public override async Task HandleAsync(ITelegramBotClient botClient, UserService userService, User user,
        long chatId, string messageText,
        CancellationToken cancellationToken)
    {
        var categoryName = messageText.Trim();

        if (string.IsNullOrWhiteSpace(categoryName) || categoryName.Length < 3)
        {
            await SendErrorAsync(botClient, chatId,
                "Название категории должно содержать хотя бы три символа и не быть пустым.", user, cancellationToken);
            return;
        }

        if (user.Metadata.Any(m =>
                m.Attribute is "Category" &&
                m.Value.Equals(categoryName, StringComparison.OrdinalIgnoreCase)))
        {
            await SendErrorAsync(botClient, chatId,
                $"Категория '{categoryName}' уже существует. Попробуйте другое название.", user, cancellationToken);
            return;
        }


        await userService.AddMetadata(user.TelegramId, "Category", categoryName);

        var text = "Категория успешно добавлена!";
        await botClient.EditMessageTextAsync(chatId, user.MainMessageId, text, replyMarkup: MainKeyboard.Back,
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