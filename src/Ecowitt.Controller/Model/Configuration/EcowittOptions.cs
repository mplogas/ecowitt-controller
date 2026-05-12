namespace Ecowitt.Controller.Model.Configuration;

public class EcowittOptions
{
    public int PollingInterval { get; set; } = 30;
    public int LiveDataInterval { get; set; } = 5;
    public bool CalculateValues { get; set; } = true;
    public int Retries { get; set; } = 2;
    public List<GatewayOptions> Gateways { get; set; } = new();
}

public class GatewayOptions
{
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public bool Subdevices { get; set; }
    public IngestMode IngestMode { get; set; } = IngestMode.Push;
}