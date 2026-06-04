using Ecowitt.Controller.Model.Api;

namespace Ecowitt.Controller.Model.Message.Data;

// Partial staged-config update from HA entities. Null fields = not updating.
// Start=true means: assemble + fire using the subdevice's staged config.
public class SubdeviceRunConfig
{
    public int Id { get; set; }
    public RunModeKey? Mode { get; set; }
    public int? Duration { get; set; }   // user minutes
    public int? Volume { get; set; }     // user liters
    public bool Start { get; set; }
}
