using BudgetManager.Application.Services;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot.States;

public static class StateHandler
{
    public static async Task HandleUserState(
        ITelegramBotClient botClient, Message message, UserService userService,
        CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var user = await userService.GetUserByTelegramIdAsync(chatId);

        if (!UserStates.State.TryGetValue(chatId, out var userState))
        {
            await SendDefaultErrorMessage(botClient, chatId, user.MainMessageId, cancellationToken);
            return;
        }

        await StateManager.HandleUserStateAsync(userState, botClient, userService, user, chatId, message.Text, cancellationToken);
    }
    
    private static async Task SendDefaultErrorMessage(ITelegramBotClient botClient, long chatId, int mainMessageId,
        CancellationToken cancellationToken)
    {
        await botClient.EditMessageTextAsync(
            chatId: chatId,
            messageId: mainMessageId,
            text: "Чего-то пошло не так, попробуй снова",
            replyMarkup: MainKeyboard.Back,
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
    }

}