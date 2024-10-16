using BudgetManager.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BudgetManager.Infrastructure.TelegramBot;

public static class TelegramBotExtensions
{
    public static IServiceCollection AddTelegramBot(this IServiceCollection services, string token)
    {
        services.AddHostedService(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<BotService>>();
            return new BotService(token, logger, provider);
        });

        return services;
    }
}