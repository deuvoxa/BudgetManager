﻿using BudgetManager.Application.Services;
using BudgetManager.Domain.Entities;
using Telegram.Bot;

namespace BudgetManager.Infrastructure.TelegramBot.States;

public abstract class UserStateBase : IUserState
{
    public abstract Task HandleAsync(ITelegramBotClient botClient, UserService userService, 
        User user, long chatId, string messageText, CancellationToken cancellationToken);
}