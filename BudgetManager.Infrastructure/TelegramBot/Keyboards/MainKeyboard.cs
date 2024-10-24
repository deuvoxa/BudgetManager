using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetManager.Infrastructure.TelegramBot.Keyboards;

public static class MainKeyboard
{
    public static KeyboardBuilder WithBackToHome(this KeyboardBuilder builder)
        => builder.WithButton("Вернуться назад", "main-menu");

    private static KeyboardBuilder WithHome(this KeyboardBuilder builder)
        => builder.WithButtons([
                ("Добавить транзакцию", "transactions-add"),
            ])
            .WithButton("Посмотреть счета", "accounts-menu")
            .WithButtons([
                ("Добавить категорию", "categories-add"),
                ("Добавить пассивы", "add-liabilities")
            ])
            .WithButton("Статистика", "statistics-menu")
            .WithButton("Удалить категорию", "categories-remove");

    public static InlineKeyboardMarkup Back => new KeyboardBuilder().WithBackToHome().Build();

    public static InlineKeyboardMarkup Home => new KeyboardBuilder().WithHome().Build();
}