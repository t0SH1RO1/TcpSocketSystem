using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TcpSocketServer.Services;

public class SocketServerService : IHostedService
{
    private readonly ILogger<SocketServerService> _logger;
    private readonly ICommandHandler _commandHandler;
    private readonly int _port;
    private TcpListener? _listener;
    private CancellationTokenSource? _cancellationTokenSource;

    public SocketServerService(
        ILogger<SocketServerService> logger,
        ICommandHandler commandHandler,
        IConfiguration configuration)
    {
        _logger = logger;
        _commandHandler = commandHandler;
        _port = configuration.GetValue<int>("ServerPort", 5000);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();

        _logger.LogInformation("Server started on port {Port}", _port);

        // Start accepting connections in background
        _ = AcceptConnectionsAsync(_cancellationTokenSource.Token);

        return Task.CompletedTask;
    }

    private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _listener!.AcceptTcpClientAsync(cancellationToken);
                var clientEndPoint = client.Client.RemoteEndPoint;
                _logger.LogInformation("Client connected: {EndPoint}", clientEndPoint);

                // Handle each client in separate task
                _ = HandleClientAsync(client, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown, ignore
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting connections");
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var clientEndPoint = client.Client.RemoteEndPoint;

        try
        {
            using (client)
            {
                await using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                await using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                while (!cancellationToken.IsCancellationRequested && client.Connected)
                {
                    // Read line (command) from client
                    var line = await reader.ReadLineAsync(cancellationToken);

                    // Client disconnected or stream closed
                    if (line == null)
                    {
                        _logger.LogInformation("Client disconnected: {EndPoint}", clientEndPoint);
                        break;
                    }

                    _logger.LogInformation("Received command: {Command} from {EndPoint}", line, clientEndPoint);

                    // Process command
                    var result = await _commandHandler.HandleCommandAsync(line, cancellationToken);
                    await writer.WriteLineAsync(result.Response.AsMemory(), cancellationToken);

                    // If client requested logout, close the connection
                    if (result.IsLogout)
                    {
                        _logger.LogInformation("Client logged out: {EndPoint}", clientEndPoint);
                        break;
                    }
                }
            }
        }
        catch (IOException ex) when (ex.InnerException is SocketException)
        {
            _logger.LogInformation("Client disconnected unexpectedly: {EndPoint}", clientEndPoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling client: {EndPoint}", clientEndPoint);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Server stopping...");
        _cancellationTokenSource?.Cancel();
        _listener?.Stop();
        return Task.CompletedTask;
    }
}
