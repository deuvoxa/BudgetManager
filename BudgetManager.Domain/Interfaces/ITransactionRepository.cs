using BudgetManager.Domain.Entities;

namespace BudgetManager.Domain.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction);
    Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId);
}