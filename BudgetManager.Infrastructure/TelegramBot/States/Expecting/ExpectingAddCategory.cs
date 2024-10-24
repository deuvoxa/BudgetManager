using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot.States.Expecting;

public class ExpectingAddCategory : UserStateBase
{
    public override async Task HandleAsync(ITelegramBotClient botClient, UserService userService, User user, long chatId, string messageText,
        CancellationToken cancellationToken)
    {
        // TODO: Проверка на наличие такой же категории
        // TODO: Проверка пользовательских данных
        var categoryName = messageText;
        
        await userService.AddMetadata(user.TelegramId, "Category", categoryName);

        var text = "Категория успешно добавлена!";
        await botClient.EditMessageTextAsync(chatId, user.MainMessageId, text, replyMarkup: MainKeyboard.Back,
            parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);

        UserStates.State[chatId] = string.Empty;
    }
}