namespace Ecowitt.Controller.Model.Configuration;

public class ControllerOptions
{
    public int Precision { get; set; } = 2;
    public Units Units { get; set; } = Units.Metric;
    public bool HomeAssistantDiscovery { get; set; } = true;
}
    
public enum Units
{
    Imperial,
    Metric
}