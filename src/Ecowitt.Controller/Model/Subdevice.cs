using Ecowitt.Controller.Model.Api;

namespace Ecowitt.Controller.Model;

public class Subdevice
{
    public int Id { get; set; }
    public string Nickname { get; set; } = default!;
    public string Devicename { get; set; } = default!;
    public SubdeviceModel Model { get; set; }
    public bool Availability { get; set; }
    public string GwIp { get; set; } = default!;
    public int Version { get; set; }
    public DateTime TimestampUtc { get; set; }
    public List<ISensor> Sensors { get; set; } = new List<ISensor>();
    public bool DiscoveryUpdate { get; set; }
    public bool HasFlowMeter { get; set; }
    public StagedRunConfig StagedRunConfig { get; set; } = new();
}

public class StagedRunConfig
{
    // POCO defaults — sensible run config before the user touches anything.
    public RunModeKey Mode { get; set; } = RunModeKey.Duration;
    public int DurationMinutes { get; set; } = 3;
    public int VolumeLiters { get; set; } = 5;
}

public enum SubdeviceModel
{
    Unknown = 0,
    WFC01 = 1,
    AC1100 = 2,
    WFC02 = 3
}