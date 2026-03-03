namespace Ecowitt.Controller.Model.Message.Event;

public class DiscoveryRemovalEvent
{
    public string DeviceName { get; set; } = string.Empty;
    public List<ISensor> Sensors { get; set; } = new();
}
