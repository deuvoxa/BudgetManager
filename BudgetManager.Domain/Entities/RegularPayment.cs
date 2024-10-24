namespace BudgetManager.Domain.Entities;

public enum PaymentType
{
    RegularExpense,
    Credit,
    Debt
}

public class RegularPayment
{
    public Guid Id { get; set; }
    public PaymentType PaymentType { get; set; } // Тип обязательства (Кредит, Долг, Ежемесячные траты)
    public decimal Amount { get; set; } // Сумма для ежемесячной оплаты
    public int PaymentDueDate { get; set; } // Дата оплаты
    public decimal? Debt { get; set; } // Задолженность (для кредита)
    public string Description { get; set; } // Описание

    // Связи с другими сущностями
    public Guid UserId { get; set; }
    public User User { get; set; }
}