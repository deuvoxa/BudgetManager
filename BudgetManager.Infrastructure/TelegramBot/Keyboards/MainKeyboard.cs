using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetManager.Infrastructure.TelegramBot.Keyboards;

public static class MainKeyboard
{
    private static KeyboardBuilder WithBackToHome(this KeyboardBuilder builder)
        => builder.WithButton("Вернуться назад", "main-menu");

    private static KeyboardBuilder WithHome(this KeyboardBuilder builder)
        => builder.WithButtons([
                ("Добавить транзакцию", "add-transaction"),
                ("Посмотреть счета", "view-accounts")
            ])
            .WithButtons([
                ("Статистика расходов", "view-stats"),
                ("Добавить пассивы", "add-liabilities")
            ]);

    public static InlineKeyboardMarkup Back => new KeyboardBuilder().WithBackToHome().Build();

    public static InlineKeyboardMarkup Home => new KeyboardBuilder().WithHome().Build();
}