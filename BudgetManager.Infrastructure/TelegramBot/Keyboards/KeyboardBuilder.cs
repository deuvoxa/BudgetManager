using Telegram.Bot.Types.ReplyMarkups;

namespace BudgetManager.Infrastructure.TelegramBot.Keyboards;

public class KeyboardBuilder
{
    private readonly List<List<InlineKeyboardButton>> _buttons = [];

    public KeyboardBuilder WithUrlButton(string text, string url)
    {
        _buttons.Add([InlineKeyboardButton.WithUrl(text, url)]);
        return this;
    }
    
    public KeyboardBuilder WithButton(string text, string callbackData)
    {
        _buttons.Add([InlineKeyboardButton.WithCallbackData(text, callbackData)]);
        return this;
    }

    public KeyboardBuilder WithButtons(IEnumerable<(string text, string callbackData)> buttons)
    {
        var buttonRow = buttons
            .Select(b
                => InlineKeyboardButton.WithCallbackData(b.text, b.callbackData)).ToList();
        _buttons.Add(buttonRow);
        return this;
    }

    public InlineKeyboardMarkup Build()
    {
        return new InlineKeyboardMarkup(_buttons);
    }
}