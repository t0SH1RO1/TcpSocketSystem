using System.CommandLine;
using Microsoft.Extensions.Logging;
using TcpSocketClient.Services;

namespace TcpSocketClient;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Setup command-line arguments
        var hostOption = new Option<string>("--host", "Host address to connect to") { IsRequired = true };
        var portOption = new Option<int>("--port", "Port to connect to") { IsRequired = true };

        var rootCommand = new RootCommand("TCP Socket Client");
        rootCommand.AddOption(hostOption);
        rootCommand.AddOption(portOption);

        rootCommand.SetHandler(async (string host, int port) =>
        {
            // Setup simple logging
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<SocketClientService>();

            // Create client service
            var clientService = new SocketClientService(logger, host, port);

            try
            {
                await clientService.ConnectAsync(CancellationToken.None);
                await clientService.RunAsync(Console.In, Console.Out, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error running client");
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1); // Use Environment.Exit instead of return
            }
        }, hostOption, portOption);

        return await rootCommand.InvokeAsync(args);
    }
}
