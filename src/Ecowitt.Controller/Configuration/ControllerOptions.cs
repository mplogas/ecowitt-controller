namespace Ecowitt.Controller.Configuration;

public class ControllerOptions
{
    public int Precision { get; set; } = 2;
    public Units Units { get; set; } = Units.Metric;
    public int PublishingInterval { get; set; } = 60;
    public bool HomeAssistantDiscovery { get; set; } = true;
}
    
public enum Units
{
    Imperial,
    Metric
}