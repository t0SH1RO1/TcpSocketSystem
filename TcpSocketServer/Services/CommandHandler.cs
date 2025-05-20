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

    public async Task<ResponseCommand> HandleCommandAsync(string command, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return new ResponseCommand("ERROR EmptyCommand");
            }

            command = command.Trim();

            if (command.Equals("PING", StringComparison.OrdinalIgnoreCase))
            {
                return new ResponseCommand("PONG");
            }

            if (command.StartsWith("CAR ", StringComparison.OrdinalIgnoreCase))
            {
                var brand = command.Substring(4).Trim();
                if (string.IsNullOrWhiteSpace(brand))
                {
                    return new ResponseCommand("ERROR MissingBrand");
                }

                var info = await _carApiService.GetCarInfoAsync(brand, cancellationToken);
                return new ResponseCommand(info);
            }

            if (command.Equals("LOGOUT", StringComparison.OrdinalIgnoreCase))
            {
                return new ResponseCommand("OK Goodbye", isLogout: true);
            }

            return new ResponseCommand($"ERROR UnknownCommand: {command}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling command: {Command}", command);
            return new ResponseCommand("ERROR InternalError");
        }
    }
}
