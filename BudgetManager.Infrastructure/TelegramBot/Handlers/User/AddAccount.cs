using BudgetManager.Application.Services;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using BudgetManager.Infrastructure.TelegramBot.States;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers.User;

public static class AddAccount
{
    public static async Task Execute(ITelegramBotClient botClient, CallbackQuery callbackQuery, UserService
        userService, CancellationToken cancellationToken)
    {
        var message = callbackQuery.Message!;
        var chatId = message.Chat.Id;
        // var user = await userService.GetUserByTelegramIdAsync(callbackQuery.From.Id);

        await botClient.EditMessageTextAsync(chatId, message.MessageId, 
            text: "Добавление счёта:\n\nВведите в формате: `название:баланс`",
            replyMarkup: MainKeyboard.Back,
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);

        UserStates.State[chatId] = "ExpectingAddAccountState";
    }
}