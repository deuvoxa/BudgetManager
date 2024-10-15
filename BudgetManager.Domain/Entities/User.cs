namespace BudgetManager.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public long TelegramId { get; set; }  // Telegram ID пользователя
    // public string Currency { get; set; }

    // Связь с другими сущностями
    public ICollection<Account> Accounts { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
}