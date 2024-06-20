namespace Ecowitt.Controller.Configuration;

public class ControllerOptions
{
    public int Precision { get; set; } = 2;
    public Units Units { get; set; } = Units.Metric;
}

public enum Units
{
    Imperial,
    Metric
}