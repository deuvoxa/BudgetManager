using BudgetManager.Application.Services;
using BudgetManager.Infrastructure.TelegramBot.Commands;
using BudgetManager.Infrastructure.TelegramBot.States;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetManager.Infrastructure.TelegramBot.Handlers;

public static class MessageHandler
{
    public static async Task BotOnMessageReceived(
        ITelegramBotClient botClient, Message message,
        UserService userService,
        ILogger<BotService> logger, CancellationToken cancellationToken)
    {
        try
        {
            var messageText = message.Text ?? string.Empty;
            
            var user = await GetOrCreateUser(userService, message, logger);

            var (command, parameters) = CommandHandler.ParseInput(messageText);

            if (!string.IsNullOrEmpty(command))
            {
                await CommandHandler.HandleCommand(botClient, message, command, parameters, userService, cancellationToken);
                return;
            }
            
            await StateHandler.HandleUserState(botClient, message, userService, cancellationToken);
            await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId,
                cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
    }
    
    private static async Task<Domain.Entities.User> GetOrCreateUser(UserService userService, Message message, ILogger<BotService> logger)
    {
        var user = await userService.GetUserByTelegramIdAsync(message.From!.Id);
        if (user is not null) return user;
        
        user = new Domain.Entities.User
        {
            TelegramId = message.From.Id,
        };
        await userService.AddAsync(user);
        logger.LogInformation($"Новый пользователь: {user.TelegramId}!");

        return user;
    }
}