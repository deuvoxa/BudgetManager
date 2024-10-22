using BudgetManager.Application.Services;
using BudgetManager.Infrastructure.TelegramBot.Handlers.Menus;
using BudgetManager.Infrastructure.TelegramBot.Handlers.User;
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

        var user = await userService.GetUserByTelegramIdAsync(callbackQuery.From.Id);
        
        var transactionHandler =
            new TransactionHandler(botClient, callbackQuery, userService, cancellationToken);
        var transferHandler = new TransferHandler(botClient, callbackQuery, user, userService, cancellationToken);

        if (data.StartsWith("category-"))
        {
            var category = data.Split("-")[1];
            await transactionHandler.SelectCategory(category);
        }
        if (data.StartsWith("account-"))
        {
            var accountId = data.Split("-")[1];
            await transactionHandler.SelectAccount(accountId);
        }
        if (data.StartsWith("select-"))
        {
            var select = data.Split("-")[1];
            await transactionHandler.SelectIncomeExpense(select);
        }
        if (data.StartsWith("source-"))
        {
            var source = int.Parse(data.Split("-")[1]);
            await transferHandler.ChooseTargetAccount(source);
        }
        if (data.StartsWith("target-"))
        {
            var target = int.Parse(data.Split("-")[1]);
            await transferHandler.EnterTransferAmount(target);
        }

        switch (data)
        {
            case "view-stats":
                await StatsHandler.ViewStats(botClient, callbackQuery, userService, cancellationToken);
                break;
            case "transfer":
                await transferHandler.ChooseSourceAccount();
                break;
            case "accept-transfer":
                await transferHandler.AcceptTransfer();
                break;
            case "accept-transaction":
                await transactionHandler.AcceptTransaction();
                break;
            case "add-transaction":
                await transactionHandler.AddTransaction();
                break;
            case "main-menu":
                await HomeMenu.ExecuteAsync(botClient, callbackQuery, userService, cancellationToken);
                break;
            case "view-accounts":
                await ViewAccounts.Execute(botClient, callbackQuery, userService, cancellationToken);
                break;
            case "add-account":
                await AddAccount.Execute(botClient, callbackQuery, userService, cancellationToken);
                break;
        }
    }
}