using BudgetManager.Domain.Entities;

namespace BudgetManager.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByTelegramIdAsync(long telegramId);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
}