using BudgetManager.Application.Extensions;
using BudgetManager.Application.Services;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using BudgetManager.Infrastructure.TelegramBot.States;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers.User;

public class SettingsHandler(
    ITelegramBotClient botClient,
    CallbackQuery callbackQuery,
    UserService userService,
    CancellationToken cancellationToken) : HandlerBase(botClient, callbackQuery, cancellationToken)
{
    private readonly CallbackQuery _callbackQuery = callbackQuery;

    public async Task AddAccount()
    {
        var message = _callbackQuery.Message!;
        var chatId = message.Chat.Id;

        await EditMessage("*Добавление счёта*:\n\nВведите в формате: `название:баланс`", MainKeyboard.Back);

        UserStates.State[chatId] = "ExpectingAddAccount";
    }

    public async Task RemoveAccount(string accountId)
    {
        var message = _callbackQuery.Message!;
        var chatId = message.Chat.Id;

        var user = await userService.GetUserByTelegramIdAsync(chatId) ?? throw new Exception();

        var accounts = user.GetActiveAccount();

        if (accountId is "")
        {
            var text = "*Удаление счёта*:\n\n" +
                       "Какой счёт удалить?";
            var tuples = accounts
                .Select(acc => ($"{acc.Name}", $"accounts-remove-{acc.Id}"))
                .ToArray();

            var keyboard = new KeyboardBuilder()
                .WithButtonGrid(tuples)
                .WithButton("Вернуться назад", "accounts-menu")
                .Build();

            await EditMessage(text, keyboard);
            return;
        }
        
        var account = accounts.First(a => a.Id == int.Parse(accountId));

        if (user.Transactions.Any(t => t.Account == account))
        {
            account.IsActive = false;
        }
        else
        {
            user.Accounts.Remove(account);
        }

        // TODO: Перед удалением уточнять у пользователя в достоверности
        // TODO: Перед удалением спрашивать, переводить ли остаток со счёта на другой счёт (если он имеется), если нет, предупреждать
        
        await userService.UpdateAsync(user);
        await EditMessage("Счёт успешно удалён.",
            new KeyboardBuilder().WithButton("Вернуться назад", "accounts-menu").Build());
    }

    public async Task AddCategory()
    {
        var message = _callbackQuery.Message!;
        var chatId = message.Chat.Id;

        await EditMessage("*Добавление своей категории*:\n\nВведите название категории:", MainKeyboard.Back);

        UserStates.State[chatId] = "ExpectingAddCategory";
    }

    public async Task RemoveCategory(string category)
    {
        var message = _callbackQuery.Message!;
        var chatId = message.Chat.Id;

        var user = await userService.GetUserByTelegramIdAsync(chatId) ?? throw new Exception();
        var categories = user.Metadata.Where(m => m.Attribute == "Category");

        if (category is "")
        {
            var text = "*Удаление категорий*:\n\n" +
                       "Какую категорию удалить?";
            var tuples = categories
                .Select(m => ($"{m.Value}", $"categories-remove-{m.Value}"))
                .ToArray();

            var keyboard = new KeyboardBuilder()
                .WithButtonGrid(tuples)
                .WithButton("Вернуться назад", "main-menu")
                .Build();

            await EditMessage(text, keyboard);
            return;
        }
        
        var metadata = user.Metadata.First(m => m.Attribute == "Category" && m.Value == category);
        user.Metadata.Remove(metadata);
        
        await userService.UpdateAsync(user);
        await EditMessage("Категория успешно удалена.",
            new KeyboardBuilder().WithButton("Вернуться назад", "main-menu").Build());
    }
}