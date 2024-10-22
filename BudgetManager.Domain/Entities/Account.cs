namespace BudgetManager.Domain.Entities;

public class Account
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }  // Тип счета (наличные, кредитка, накопительный)
    
    // Связи с другими сущностями
    public User User { get; set; }
}

public enum AccountType
{
    Cash,
    CreditCard,
    Savings
}