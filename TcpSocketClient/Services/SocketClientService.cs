using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TcpSocketClient.Services;

public class SocketClientService
{
    private readonly ILogger<SocketClientService> _logger;
    private readonly string _host;
    private readonly int _port;
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public SocketClientService(ILogger<SocketClientService> logger, string host, int port)
    {
        _logger = logger;
        _host = host;
        _port = port;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(_host, _port, cancellationToken);
        
        var stream = _client.GetStream();
        _reader = new StreamReader(stream, Encoding.UTF8);
        _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        
        _logger.LogInformation("Connected to server at {Host}:{Port}", _host, _port);
    }

    public async Task RunAsync(TextReader input, TextWriter output, CancellationToken cancellationToken)
    {
        if (_client == null || _reader == null || _writer == null || !_client.Connected)
        {
            throw new InvalidOperationException("Client not connected. Call ConnectAsync first.");
        }

        // Create a task to read server responses
        var readServerTask = ReadServerResponsesAsync(output, cancellationToken);
        
        _logger.LogInformation("Client ready. Enter commands (type 'LOGOUT' to disconnect):");
        output.WriteLine("Client ready. Enter commands (type 'LOGOUT' to disconnect):");

        try
        {
            // Read commands from input and send to server
            string? line;
            while ((line = await input.ReadLineAsync(cancellationToken)) != null)
            {
                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Send command to server
                await _writer.WriteLineAsync(line.AsMemory(), cancellationToken);
                
                // If LOGOUT command, exit the loop
                if (line.Equals("LOGOUT", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Logout command sent, disconnecting...");
                    break;
                }
            }
        }
        finally
        {
            // Close connection
            _client.Close();
            _logger.LogInformation("Connection closed.");
        }

        // Wait for the server response reader task to complete
        await readServerTask;
    }

    private async Task ReadServerResponsesAsync(TextWriter output, CancellationToken cancellationToken)
    {
        try
        {
            while (_client!.Connected && !cancellationToken.IsCancellationRequested)
            {
                var response = await _reader!.ReadLineAsync(cancellationToken);
                
                // If response is null, the server closed the connection
                if (response == null)
                {
                    _logger.LogInformation("Server closed the connection.");
                    break;
                }
                
                // Write response to output
                await output.WriteLineAsync($"Server: {response}");
            }
        }
        catch (IOException ex) when (ex.InnerException is SocketException)
        {
            _logger.LogInformation("Connection closed by server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading server responses");
        }
    }
}