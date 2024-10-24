using BudgetManager.Domain.Entities;
using BudgetManager.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BudgetManager.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByTelegramIdAsync(long telegramId)
        => await context.Users
            .Include(u => u.Accounts)
            .Include(u => u.Transactions)
            .Include(u => u.Metadata)
            .Include(u => u.RegularPayments)
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId);

    public async Task<User> AddAsync(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        return user;
    }

    public async Task UpdateAsync(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }
}