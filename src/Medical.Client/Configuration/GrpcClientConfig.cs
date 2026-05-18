namespace Medical.Client.Configuration;

public class GrpcClientConfig
{
    public string BaseAddress { get; set; } = string.Empty;
    public bool UseTls { get; set; }
    public int Timeout { get; set; } = 30;
}