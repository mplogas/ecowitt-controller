namespace Ecowitt.Controller.Configuration;

public class EcowittOptions
{
    public List<GatewayOptions> Gateways { get; set; } = new();
}

public class GatewayOptions
{
    public string Name { get; set; }
    public string Ip { get; set; }
    public int Port { get; set; } = 80;
    public int Retries { get; set; } = 2;
}