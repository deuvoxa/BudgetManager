using BudgetManager.Application.Services;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using BudgetManager.Infrastructure.TelegramBot.States;
using Telegram.Bot;
using Telegram.Bot.Types;
namespace BudgetManager.Infrastructure.TelegramBot.Handlers.User;

public class LiabilitiesHandler(
    ITelegramBotClient botClient,
    CallbackQuery callbackQuery,
    UserService userService,
    CancellationToken cancellationToken) : HandlerBase(botClient, callbackQuery, cancellationToken)
{
    private readonly CallbackQuery _callbackQuery = callbackQuery;

    public async Task AddLiabilities()
    {
        var text = "Какой ежемесячный платёж добавляем?";
        var keyboard = new KeyboardBuilder()
            .WithButton("Ежемесячный платёж", "add-liabilities-regular")
            .WithButtons([
                ("Кредит", "add-liabilities-credit"),
                ("Долг", "add-liabilities-debt"),
            ])
            .WithButton("Вернуться назад", "main-menu")
            .Build();

        await EditMessage(text, keyboard);
    }

    public async Task AddLiabilities(string liabilities)
    {
        var text = liabilities switch
        {
            "credit" => "Для добавления _кредита_, укажите данные в формате:\n" +
                        "`<задолженность>:<число платежа>:<сумма платежа>`\n" + "например: `30000:11:3000`",
            "debt" => "Для добавления _долга_, укажите данные в формате:\n" + "`<задолженность>:<число платежа>`\n" +
                      "например: `1000:15`",
            "regular" => "Для добавления _регулярных трат_, укажите данные в формате:\n" +
                         "`<число платежа>:<сумма платежа>`\n" + "например: `22:500`",
            _ => ""
        };

        await userService.AddMetadata(_callbackQuery.From.Id, "Liabilities", liabilities);
        
        var chatId = _callbackQuery.Message!.Chat.Id;
        UserStates.State[chatId] = "ExpectingAddLiabilities";
        
        await EditMessage(text, MainKeyboard.Back);
    }
}