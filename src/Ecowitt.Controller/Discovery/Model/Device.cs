namespace Ecowitt.Controller.Discovery.Model;

public class Device
{
    public List<string> Identifiers { get; set; } = new List<string>();
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string Name { get; set; }
    public string Hw_Version { get; set; }
    public string Sw_Version { get; set; }
    public string? Via_Device { get; set; }
}