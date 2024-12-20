﻿using BudgetManager.Domain.Entities;

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
    
    public static IEnumerable<Transaction> GetTransactionsForCurrentMonth(this User user)
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var endOfToday = DateTime.UtcNow;

        return user.Transactions.Where(t => t.Date >= startOfMonth && t.Date <= endOfToday);
    }    
    public static IEnumerable<Transaction> GetTransactionsForCurrentWeek(this User user)
    {
        var currentDayOfWeek = (int)DateTime.UtcNow.DayOfWeek;
        var startOfWeek = DateTime.UtcNow.AddDays(-currentDayOfWeek + 1).Date; 
        var endOfToday = DateTime.UtcNow;

        return user.Transactions.Where(t => t.Date >= startOfWeek && t.Date <= endOfToday);
    }
    public static IEnumerable<Transaction> GetTransactionsForCurrentDay(this User user)
    {
        var startOfDay = DateTime.UtcNow.Date;
        var endOfToday = DateTime.UtcNow;

        return user.Transactions.Where(t => t.Date >= startOfDay && t.Date <= endOfToday);
    }

    public static IEnumerable<Transaction> GetTransactionsByCategoryForCurrentMonth(this User user, string category)
    {
        var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var endOfToday = DateTime.Now;

        return user.Transactions.Where(t => t.Category == category && t.Date >= startOfMonth && t.Date <= endOfToday);
    }

    public static decimal GetTotalByCategoryForCurrentMonth(this User user, string category)
    {
        var transactions = GetTransactionsByCategoryForCurrentMonth(user, category);
        return transactions.Sum(t => t.Amount);
    }
}