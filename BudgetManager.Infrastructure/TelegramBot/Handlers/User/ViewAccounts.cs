using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers.User;

public static class ViewAccounts
{
    public static async Task Execute(ITelegramBotClient botClient, CallbackQuery callbackQuery, UserService
        userService, CancellationToken cancellationToken)
    {
        var user = await userService.GetUserByTelegramIdAsync(callbackQuery.From.Id);

        var account = new Account()
        {

        };
        user.Accounts.Add(account);
    }
}