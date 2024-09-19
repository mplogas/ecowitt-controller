namespace Ecowitt.Controller.Configuration;

public class EcowittOptions
{
    public int PollingInterval { get; set; } = 30;
    public bool AutoDiscovery { get; set; }
    public bool CalculateValues { get; set; } = true;
    public int Retries { get; set; } = 2;
    public List<GatewayOptions> Gateways { get; set; } = new();
}

public class GatewayOptions
{
    public string Passkey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Ip { get; set; }
    
}