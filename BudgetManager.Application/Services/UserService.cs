using BudgetManager.Domain.Entities;
using BudgetManager.Domain.Interfaces;

namespace BudgetManager.Application.Services;

public class UserService(IUserRepository userRepository)
{
    public async Task<User?> GetUserByTelegramIdAsync(long telegramId)
        => await userRepository.GetByTelegramIdAsync(telegramId);

    public async Task<User> AddUserAsync(User user)
        => await userRepository.AddAsync(user);
}