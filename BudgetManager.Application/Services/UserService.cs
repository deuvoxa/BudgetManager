using BudgetManager.Domain.Entities;
using BudgetManager.Domain.Interfaces;

namespace BudgetManager.Application.Services;

public class UserService(IUserRepository userRepository)
{
    public async Task<User?> GetUserByTelegramIdAsync(long telegramId)
        => await userRepository.GetByTelegramIdAsync(telegramId);

    public async Task<User> AddAsync(User user)
        => await userRepository.AddAsync(user);

    public async Task UpdateAsync(User user)
        => await userRepository.UpdateAsync(user);

    public async Task AddMetadata(long telegramId, string attribute, string value)
    {
        var user = await userRepository.GetByTelegramIdAsync(telegramId);
        var metadata = new UserMetadata
        {
            UserId = user.Id,
            Attribute = attribute,
            Value = value
        };
 
        user.Metadata.Add(metadata);
        await userRepository.UpdateAsync(user);
    }
    
    public async Task RemoveMetadata(long telegramId, string attribute)
    {
        var user = await userRepository.GetByTelegramIdAsync(telegramId);
        var metadata = user.Metadata.FirstOrDefault(m => m.Attribute == attribute);
        if (metadata is null)
        {
            return;
        }
        user.Metadata.Remove(metadata);
        await userRepository.UpdateAsync(user);
    }
}