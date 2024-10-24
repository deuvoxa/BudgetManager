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
        var liabilities = user.Metadata.FirstOrDefault(m => m.Attribute is "Liabilities")?.Value;
        var paymentType = liabilities switch
        {
            "regular" => PaymentType.RegularExpense,
            "credit" => PaymentType.Credit,
            "debt" => PaymentType.Debt
        };

        // TODO: Добавить проверку параметров от пользователя
        var parameters = messageText.Split(":");
        
        var amount = paymentType switch
        {
            PaymentType.RegularExpense => parameters[1],
            PaymentType.Credit => parameters[2],
            PaymentType.Debt => parameters[0],
        };        
        var paymentDate = paymentType switch
        {
            PaymentType.RegularExpense => parameters[0],
            PaymentType.Credit => parameters[1],
            PaymentType.Debt => parameters[1],
        };        
        var debt = paymentType switch
        {
            PaymentType.RegularExpense => parameters[1],
            PaymentType.Credit => parameters[2],
            PaymentType.Debt => parameters[0],
        };        

        var regularPayment = new RegularPayment()
        {
            PaymentType = paymentType,
            // TODO: Реализация описания для регулярных платежей
            Description = "",
            Amount = decimal.Parse(amount),
            PaymentDueDate = short.Parse(paymentDate),
            Debt =  decimal.Parse(debt)
        };
        
        user.RegularPayments.Add(regularPayment);
        await userService.UpdateAsync(user);

        var text = "Пассив успешно добавлен.";
        
        await botClient.EditMessageTextAsync(chatId, user.MainMessageId, text, replyMarkup: MainKeyboard.Back,
            parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);

        UserStates.State[chatId] = string.Empty;
    }
}