using BudgetManager.Application.Services;
using BudgetManager.Infrastructure.TelegramBot.States.Expecting;
using Telegram.Bot;

namespace BudgetManager.Infrastructure.TelegramBot.States;

public static class StateManager
{
    private static readonly Dictionary<string, IUserState> States = new()
    {
        { "ExpectingAddAccount", new ExpectingAddAccount() },
        { "ExpectingAmountTransaction", new ExpectingTransactionAmount()},
        { "ExpectingTransferAmount", new ExpectingTransferAmount()},
        { "ExpectingAddCategory", new ExpectingAddCategory()},
        { "ExpectingAddLiabilities", new ExpectingAddLiabilities()},
    };

    public static async Task HandleUserStateAsync(
        string state, ITelegramBotClient botClient, UserService userService, 
        Domain.Entities.User user, long chatId, string messageText, 
        CancellationToken cancellationToken)
    {
        if (States.TryGetValue(state, out var userState))
        {
            await userState.HandleAsync(botClient, userService, user, chatId, messageText, cancellationToken);
        }
    }
}