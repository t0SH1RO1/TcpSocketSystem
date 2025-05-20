namespace TcpSocketServer.Services;

public class ResponseCommand
{
    public string Response { get; set; }
    public bool IsLogout { get; set; }

    public ResponseCommand(string response, bool isLogout = false)
    {
        Response = response;
        IsLogout = isLogout;
    }
}
