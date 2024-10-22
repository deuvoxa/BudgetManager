namespace BudgetManager.Domain.Entities;

public enum PaymentType
{
    RegularExpense,   // Ежемесячные траты
    Credit,           // Кредит
    Debt              // Долг
}

public class RegularPayment
{
    public Guid Id { get; set; }
    public PaymentType PaymentType { get; set; } // Тип обязательства (Кредит, Долг, Ежемесячные траты)
    public decimal Amount { get; set; } // Сумма для ежемесячной оплаты
    public DateTime PaymentDueDate { get; set; } // Дата оплаты
    public decimal? InterestRate { get; set; } // Процентная ставка (для кредита)
    public decimal? MonthlyPayment { get; set; } // Ежемесячный платёж (для кредита)
    public DateTime? RepaymentDate { get; set; } // Срок погашения (для долгов)
    public string Description { get; set; } // Описание обязательства

    // Связи с другими сущностями
    public Guid UserId { get; set; }
    public User User { get; set; }
}