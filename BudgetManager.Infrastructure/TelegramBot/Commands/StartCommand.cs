using BudgetManager.Application.Services;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot.Commands;

public static class StartCommand
{
    public static async Task ExecuteAsync(
        ITelegramBotClient botClient, Message message, string[] parameters,
        UserService userService, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;

        const string text = """
                            *Главное меню*

                            Добро пожаловать!
                            _Пожалуйста, выберите, что вас интересует:_
                            """;
        
        var user = await userService.GetUserByTelegramIdAsync(message.From.Id);
        

        var startMessage = await botClient.SendTextMessageAsync(
            chatId, text,
            replyMarkup: MainKeyboard.Home,
                parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
        
        user.MainMessageId = startMessage.MessageId;
        await userService.UpdateAsync(user);
    }
}