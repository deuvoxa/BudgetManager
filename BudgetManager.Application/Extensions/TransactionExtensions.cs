using BudgetManager.Domain.Entities;

namespace BudgetManager.Application.Extensions;

public static class TransactionExtensions
{
    public static IEnumerable<Transaction> GetTransactionsByPeriod(this User user, DateTime start, DateTime end)
    {
        return user.Transactions.Where(t => t.Date >= start && t.Date <= end);
    }

    public static IEnumerable<Transaction> GetTransactionsByCategory(this User user, string category, DateTime start, DateTime end)
        => user.Transactions.Where(t => t.Category == category && t.Date >= start && t.Date <= end);

    public static decimal GetTotalByCategory(this User user, string category, DateTime start, DateTime end)
    {
        var transactions = GetTransactionsByCategory(user, category, start, end);
        return transactions.Sum(t => t.Amount);
    }

    public static Dictionary<string, decimal> GetStatisticIncomeByCategories(this User user, DateTime start, DateTime end)
    {
        return user.Transactions
            .Where(t => t.Date >= start && t.Date <= end && t.Type == TransactionType.Income)
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
    }
    public static Dictionary<string, decimal> GetStatisticExpenseByCategories(this User user, DateTime start, DateTime end)
    {
        return user.Transactions
            .Where(t => t.Date >= start && t.Date <= end && t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
    }

    public static decimal GetPredictedExpenses(this User user, string category, int daysAhead)
    {
        var lastTransactions = GetTransactionsByCategory(user, category, DateTime.Now.AddMonths(-3), DateTime.Now);
        var dailyAverage = lastTransactions.Average(t => t.Amount);
        return dailyAverage * daysAhead;
    }
}