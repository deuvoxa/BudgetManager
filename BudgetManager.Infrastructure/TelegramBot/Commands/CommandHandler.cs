using Telegram.Bot;
using Telegram.Bot.Types;

namespace BudgetManager.Infrastructure.TelegramBot.Commands;

public class CommandHandler
{
    private static readonly List<string> Commands =
    [
        "/start",
    ];

    public static (string command, string[] parameters) ParseInput(string input)
    {
        foreach (var command in Commands)
        {
            if (!input.StartsWith(command)) continue;
            var parameters = input[command.Length..].Trim();
            return (command, parameters.Split(' '));
        }

        return (string.Empty, input.Split(' '));
    }

    public static async Task HandleCommand(
        ITelegramBotClient botClient, Message message, string command, string[] parameters, 
        CancellationToken cancellationToken)
    {
        switch (command)
        {
            case "/start":
                await StartCommand.ExecuteAsync(botClient, message, parameters, cancellationToken);
                break;
            default:
                await botClient.DeleteMessageAsync(message.Chat, message.MessageId, cancellationToken);
                break;
        }
    }
}