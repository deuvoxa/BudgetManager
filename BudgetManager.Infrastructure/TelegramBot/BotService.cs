using BudgetManager.Application.Services;
using BudgetManager.Infrastructure.TelegramBot.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetManager.Infrastructure.TelegramBot;

public class BotService(
    string token,
    ILogger<BotService> logger,
    IServiceProvider serviceProvider
)
    : IHostedService
{
    private readonly ITelegramBotClient _botClient = new TelegramBotClient(token);
    private CancellationTokenSource _cts = null!;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Telegram bot service...");
        _cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // Получать все типы обновлений
        };

        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: _cts.Token
        );

        logger.LogInformation("Telegram bot started.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Telegram bot service...");
        _cts.Cancel();
        logger.LogInformation("Telegram bot stopped.");
        return Task.CompletedTask;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();
            var handler = update switch
            {
                { Message: { } message } =>
                    MessageHandler.BotOnMessageReceived(botClient, message, userService, logger, cancellationToken),
                { CallbackQuery: { } callbackQuery } =>
                    CallbackQueryHandler.BotOnCallbackQueryReceived(callbackQuery, botClient, userService,
                        logger, cancellationToken)
            };

            await handler;
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error: {apiRequestException.ErrorCode}\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogError(errorMessage);
        return Task.CompletedTask;
    }
}