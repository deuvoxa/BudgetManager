using BudgetManager.Application.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers;

public static class CallbackQueryHandler
{
    public static async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, ITelegramBotClient botClient,
        UserService userService, ILogger<BotService> logger, CancellationToken cancellationToken)
    {
        var data = callbackQuery.Data!;

        if (data.StartsWith("admin-"))
        {
            // await AdminCallbackHandler.HandleAdminCallback(botClient, callbackQuery, userService,
            //     ticketService, serverService, cancellationToken);
        }
        else
        {
            // await UnknownCallbackHandler.HandleUnknownCallback(callbackQuery, cancellationToken);
        }
    }
}