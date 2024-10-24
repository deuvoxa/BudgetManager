namespace BudgetManager.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }  // Внешний ключ на пользователя
    public int AccountId { get; set; }  // Внешний ключ на счет
    public decimal Amount { get; set; }  // Сумма транзакции
    public TransactionType Type { get; set; }  // Тип транзакции (доход/расход)
    public DateTime Date { get; set; }  // Дата транзакции
    public string Category { get; set; }  // Категория транзакции
    public string Description { get; set; }  // Описание

    // Связи с другими сущностями
    public User User { get; set; }
    public Account Account { get; set; }
}

public enum TransactionType
{
    Income,
    Expense
}