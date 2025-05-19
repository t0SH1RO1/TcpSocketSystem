namespace TcpSocketServer.Services;

public interface ICarApiService
{
    Task<string> GetCarInfoAsync(string brand, CancellationToken cancellationToken);
}