namespace TcpSocketServer.Services;

public interface ICommandHandler
{
    Task<string> HandleCommandAsync(string command, CancellationToken cancellationToken);
}