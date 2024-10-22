using BudgetManager.Application.Services;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using BudgetManager.Infrastructure.TelegramBot.States;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers.Menus;

public class HomeMenu
{
    public static async Task ExecuteAsync(
        ITelegramBotClient botClient, CallbackQuery callbackQuery,
        UserService userService, CancellationToken cancellationToken)
    {
        var message = callbackQuery.Message!;
        var chatId = message.Chat.Id;

        const string text = """
                            *Главное меню*

                            Добро пожаловать!
                            _Пожалуйста, выберите, что вас интересует:_
                            """;

        var user = await userService.GetUserByTelegramIdAsync(chatId);
        
        // Удаление состояний и метаданных
        
        UserStates.State[chatId] = string.Empty;
        await userService.RemoveMetadata(chatId, "amount");
        await userService.RemoveMetadata(chatId, "accountId");
        await userService.RemoveMetadata(chatId, "isIncome");
        await userService.RemoveMetadata(chatId, "category");
        await userService.RemoveMetadata(chatId, "AmountTransfer");
        await userService.RemoveMetadata(chatId, "TargetAccountId");
        await userService.RemoveMetadata(chatId, "SourceAccountId");
        
        await botClient.EditMessageTextAsync(
            chatId, user.MainMessageId, text,
            replyMarkup: MainKeyboard.Home,
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
    }
}