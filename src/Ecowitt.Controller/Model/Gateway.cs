namespace Ecowitt.Controller.Model;

public class Gateway
{
    // important properties
    public string IpAddress { get; set; }
    public string Name { get; set; }
    public DateTime TimestampUtc { get; set; }
    public List<Subdevice> Subdevices { get; set; } = new();
    public bool DiscoveryUpdate { get; set; }
    
    // system properties
    public string? Model { get; set; } 
    public string? PASSKEY { get; set; } 
    public string? StationType { get; set; } 
    public int? Runtime { get; set; }

    // i don't think we need the time of the GW for anything
    // public DateTime? DateUtc { get; set; }
    public string? Freq { get; set; }

    // sensors
    public List<ISensor> Sensors { get; set; } = new List<ISensor>();
}