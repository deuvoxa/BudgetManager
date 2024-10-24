using BudgetManager.Domain.Entities;

namespace BudgetManager.Application.Extensions;

public static class UserExtensions
{
    public static List<Account> GetActiveAccount(this User user)
        => user.Accounts.Where(a => a.IsActive is true).ToList();
}