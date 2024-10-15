using BudgetManager.Domain.Entities;

namespace BudgetManager.Domain.Interfaces;

public interface IAccountRepository
{
    Task<Account> GetByIdAsync(Guid accountId);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
}