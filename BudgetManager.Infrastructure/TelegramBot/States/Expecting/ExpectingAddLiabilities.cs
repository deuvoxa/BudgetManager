using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using BudgetManager.Infrastructure.TelegramBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot.States.Expecting;

public class ExpectingAddLiabilities : UserStateBase
{
    public override async Task HandleAsync(ITelegramBotClient botClient, UserService userService, User user,
        long chatId, string messageText, CancellationToken cancellationToken)
    {
        var liabilities = user.Metadata.FirstOrDefault(m => m.Attribute == "Liabilities")?.Value;
        
        if (!TryGetPaymentType(liabilities, out var paymentType))
        {
            await SendErrorAsync(botClient, chatId, "Ошибка: Некорректный тип платежа.", user, cancellationToken);
            return;
        }

        // Разделение параметров
        var parameters = messageText.Split(":");
        var (isValid, amount, paymentDate, debt) = ValidateParameters(parameters, paymentType);
        if (!isValid)
        {
            await SendErrorAsync(botClient, chatId, GetParameterErrorMessage(liabilities), user, cancellationToken);
            return;
        }

        // Проверка корректности параметров
        if (!decimal.TryParse(amount, out var parsedAmount) || parsedAmount <= 0 ||
            !short.TryParse(paymentDate, out var parsedDate) || parsedDate < 1 || parsedDate > 31 ||
            !decimal.TryParse(debt, out var parsedDebt) || parsedDebt < 0)
        {
            await SendErrorAsync(botClient, chatId, "Ошибка: Некорректные параметры. Проверьте ввод и попробуйте снова.", user, cancellationToken);
            return;
        }
        
        var regularPayment = new RegularPayment
        {
            PaymentType = paymentType,
            Description = "",  // TODO: Заполнить описание для регулярных платежей
            Amount = parsedAmount,
            PaymentDueDate = parsedDate,
            Debt = parsedDebt
        };

        user.RegularPayments.Add(regularPayment);
        await userService.UpdateAsync(user);

        // Уведомление об успешном добавлении пассива
        await botClient.EditMessageTextAsync(
            chatId, user.MainMessageId,
            "Пассив успешно добавлен.",
            replyMarkup: MainKeyboard.Back,
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken
        );

        UserStates.State[chatId] = string.Empty;
    }

    private static bool TryGetPaymentType(string liabilities, out PaymentType paymentType) =>
        liabilities switch
        {
            "regular" => (paymentType = PaymentType.RegularExpense) is PaymentType,
            "credit" => (paymentType = PaymentType.Credit) is PaymentType,
            "debt" => (paymentType = PaymentType.Debt) is PaymentType,
            _ => (paymentType = default) == default
        };

    private static (bool isValid, string amount, string paymentDate, string debt) ValidateParameters(string[] parameters, PaymentType paymentType)
    {
        string amount = null, paymentDate = null, debt = null;

        switch (paymentType)
        {
            case PaymentType.RegularExpense when parameters.Length == 2:
                amount = parameters[1];
                paymentDate = parameters[0];
                debt = parameters[1]; // Для регулярных платежей задолженность такая же, как и сумма
                return (true, amount, paymentDate, debt);

            case PaymentType.Credit when parameters.Length == 3:
                amount = parameters[2];
                paymentDate = parameters[1];
                debt = parameters[2]; // Задолженность для кредита
                return (true, amount, paymentDate, debt);

            case PaymentType.Debt when parameters.Length == 2:
                amount = parameters[0];
                paymentDate = parameters[1];
                debt = parameters[0]; // Для долга задолженность равна сумме
                return (true, amount, paymentDate, debt);

            default: return (false, null, null, null);
        }
    }


    private static string GetParameterErrorMessage(string liabilities)
    {
        var text = liabilities switch
        {
            "credit" => "Для добавления _кредита_, укажите данные в формате:\n" +
                        "`<задолженность>:<число платежа>:<сумма платежа>`\n" +
                        "например: `30000:11:3000`",
            "debt" => "Для добавления _долга_, укажите данные в формате:\n" +
                      "`<задолженность>:<число платежа>`\n" +
                      "например: `1000:15`",
            "regular" => "Для добавления _регулярных трат_, укажите данные в формате:\n" +
                         "`<число платежа>:<сумма платежа>`\n" +
                         "например: `22:500`",
            _ => "Ошибка: Некорректный тип пассива."
        };
    
        return text;
    }

    private async Task SendErrorAsync(ITelegramBotClient botClient, long chatId, string errorMessage, User user, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.EditMessageTextAsync(
                chatId, user.MainMessageId,
                $"{errorMessage}\n\nПопробуйте снова.",
                replyMarkup: new KeyboardBuilder().WithButton("Вернуться назад", "add-liabilities").Build(),
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        catch
        {
            // ignored
        }
    }
}
