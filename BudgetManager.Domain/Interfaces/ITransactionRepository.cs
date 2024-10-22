using BudgetManager.Domain.Entities;

namespace BudgetManager.Domain.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction> AddAsync(Transaction transaction);
    Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId);
}