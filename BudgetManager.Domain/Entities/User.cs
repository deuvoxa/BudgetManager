namespace BudgetManager.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public long TelegramId { get; set; }  // Telegram ID пользователя
    // public string Currency { get; set; }
    
    public int MainMessageId { get; set; }

    // Связь с другими сущностями
    public ICollection<Account> Accounts { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];

    public ICollection<UserMetadata> Metadata { get; set; } = [];
}

public class UserMetadata
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } // Навигационное свойство
    public string Attribute { get; set; }
    public string Value { get; set; }
}