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
        
        var settingsHandler = new SettingsHandler(botClient, callbackQuery, userService, cancellationToken);
        var liabilitiesHandler = new LiabilitiesHandler(botClient, callbackQuery, userService, cancellationToken);

        if (data is "main-menu") await HomeMenu.ExecuteAsync(botClient, callbackQuery, userService, cancellationToken);
        if (data.StartsWith("accounts-"))
        {
            var method = data.Split("-")[1];
            var parameter = data.Split("-").ElementAtOrDefault(2) ?? "";

            switch (method)
            {
                case "menu":
                    await AccountsMenu.View(botClient, callbackQuery, userService, cancellationToken);
                    break;
                case "add":
                    await settingsHandler.AddAccount();
                    break;
                case "remove":
                    await settingsHandler.RemoveAccount(parameter);
                    break;
            }
        }

        if (data.StartsWith("transactions-"))
        {
            var transactionHandler = new TransactionHandler(botClient, callbackQuery, userService, cancellationToken);

            var method = data.Split("-")[1];
            var parameter = data.Split("-").ElementAtOrDefault(2) ?? "";

            switch (method)
            {
                case "accept":
                    await transactionHandler.AcceptTransaction();
                    break;
                case "add":
                    await transactionHandler.AddTransaction();
                    break;
                case "selectCategory":
                    await transactionHandler.SelectCategory(parameter);
                    break;
                case "selectAccount":
                    await transactionHandler.SelectAccount(parameter);
                    break;
                case "select":
                    await transactionHandler.SelectIncomeExpense(parameter);
                    break;
            }
        }

        if (data.StartsWith("transfers-"))
        {
            var transferHandler = new TransferHandler(botClient, callbackQuery, user, userService, cancellationToken);
            
            var method = data.Split("-")[1];
            var parameter = data.Split("-").ElementAtOrDefault(2) ?? "";

            switch (method)
            {
                case "transfer":
                    await transferHandler.ChooseSourceAccount();
                    break;
                case "accept":
                    await transferHandler.AcceptTransfer();
                    break;
                case "selectSource":
                    await transferHandler.ChooseTargetAccount(int.Parse(parameter));
                    break;
                case "selectTarget":
                    await transferHandler.EnterTransferAmount(int.Parse(parameter));
                    break;
            }
        }

        if (data.StartsWith("add-liabilities-"))
        {
            var liabilities = data.Split("-")[2];
            await liabilitiesHandler.AddLiabilities(liabilities);
        }

        if (data.StartsWith("statistics-"))
        {
            var statisticHandler = new StatisticsHandler(botClient, callbackQuery, userService, cancellationToken);

            var parameters = data.Split("-");
            var method = parameters[1];
            var transaction = parameters.ElementAtOrDefault(2) ?? "";
            var period = parameters.ElementAtOrDefault(3) ?? "";

            switch (method)
            {
                case "menu":
                    await StatisticsMenu.ExecuteAsync(botClient, callbackQuery, userService, cancellationToken);
                    break;
                case "get":
                    await statisticHandler.GetStatistics(transaction, period);
                    break;
            }
        }

        if (data.StartsWith("categories-"))
        {
            var method = data.Split("-")[1];
            var parameter = data.Split("-").ElementAtOrDefault(2) ?? "";

            switch (method)
            {
                case "add":
                    await settingsHandler.AddCategory();
                    break;
                case "remove":
                    await settingsHandler.RemoveCategory(parameter);
                    break;
            }
        }
        

        switch (data)
        {
            case "add-liabilities":
                await liabilitiesHandler.AddLiabilities();
                break;
        }
    }
}