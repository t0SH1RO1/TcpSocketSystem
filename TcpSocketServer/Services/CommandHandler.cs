using Microsoft.Extensions.Logging;

namespace TcpSocketServer.Services;

public class CommandHandler : ICommandHandler
{
    private readonly ILogger<CommandHandler> _logger;
    private readonly ICarApiService _carApiService;

    public CommandHandler(ILogger<CommandHandler> logger, ICarApiService carApiService)
    {
        _logger = logger;
        _carApiService = carApiService;
    }

    public async Task<string> HandleCommandAsync(string command, CancellationToken cancellationToken)
    {
        try
        {
            var tokens = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                return "ERROR EmptyCommand";
            }

            return tokens[0].ToUpperInvariant() switch
            {
                "LOGOUT" => "OK Goodbye",
                "CAR" => await HandleCarCommandAsync(tokens.Skip(1).ToArray(), cancellationToken),
                "PING" => "PONG",
                _ => $"ERROR UnknownCommand"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling command: {Command}", command);
            return "ERROR InternalError";
        }
    }

    private async Task<string> HandleCarCommandAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            return "ERROR MissingCarParameter";
        }

        string brand = args[0];
        return await _carApiService.GetCarInfoAsync(brand, cancellationToken);
    }
}
