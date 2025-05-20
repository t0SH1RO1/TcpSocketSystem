namespace TcpSocketServer.Services;

public interface ICommandHandler
{
    Task<ResponseCommand> HandleCommandAsync(string command, CancellationToken cancellationToken);

}
