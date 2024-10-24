using BudgetManager.Application.Services;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers.Menus;

public static class SettingsMenu
{
    public static async Task View(ITelegramBotClient botClient, CallbackQuery callbackQuery, UserService userService,
        CancellationToken cancellationToken)
    {
        var text = "*Меню настроек:*";
        var keyboard = new KeyboardBuilder().Build();
        
    }
}