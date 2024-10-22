using BudgetManager.Application.Extensions;
using BudgetManager.Application.Services;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers.User;

public static class StatsHandler
{
    public static async Task ViewStats(ITelegramBotClient botClient, CallbackQuery callbackQuery,
        UserService userService, CancellationToken cancellationToken)
    {
        var message = callbackQuery.Message!;
        var chatId = message.Chat.Id;
        var user = await userService.GetUserByTelegramIdAsync(callbackQuery.From.Id);
        var statistics = user.GetStatisticExpenseByCategories(DateTime.UtcNow.AddMonths(-1), DateTime.Now);

        var text = "Статистика за последний месяц:\n" +
                   string.Join("\n", statistics.Select(stat => $"{stat.Key}: {stat.Value}"));
        await botClient.EditMessageTextAsync(chatId, message.MessageId, text,
            replyMarkup: MainKeyboard.Back, cancellationToken: cancellationToken);
    }
}