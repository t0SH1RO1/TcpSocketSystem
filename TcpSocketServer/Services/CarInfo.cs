namespace TcpSocketServer.Services;

public class CarInfo
{
    public string make { get; set; } = string.Empty;
    public string model { get; set; } = string.Empty;
    public int year { get; set; }
    public string class_name { get; set; } = string.Empty;
    public double? displacement { get; set; }
    public int? cylinders { get; set; }
    public string transmission { get; set; } = string.Empty;
    public string drive { get; set; } = string.Empty;
}
