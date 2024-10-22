using BudgetManager.Application.Services;
using Telegram.Bot;

namespace BudgetManager.Infrastructure.TelegramBot.States;

public static class StateManager
{
    private static readonly Dictionary<string, IUserState> _states = new()
    {
        { "ExpectingAddAccountState", new ExpectingAddAccountState() },
        { "ExpectingAmountTransactionState", new ExpectingAmountTransactionState()},
        { "ExpectingTransferAmount", new ExpectingTransferAmountState()}
        // { "ExpectingCountDays", new ExpectingCountDaysState() },
        // { "ExpectingReplyQuestion", new ExpectingReplyQuestionState() },
    };

    public static async Task HandleUserStateAsync(
        string state, ITelegramBotClient botClient, UserService userService, 
        Domain.Entities.User user, long chatId, string messageText, 
        CancellationToken cancellationToken)
    {
        if (_states.TryGetValue(state, out var userState))
        {
            await userState.HandleAsync(botClient, userService, user, chatId, messageText, cancellationToken);
        }
    }
}