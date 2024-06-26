namespace Ecowitt.Controller.Model;

public class SubdeviceCommand
{
    public Command Cmd { get; set; }
    public int Id { get; set; }
    public int? Duration { get; set; }
    public DurationUnit? Unit { get; set; }
    public bool? AlwaysOn { get; set; }
}

public enum Command
{
    Start,
    Stop
}

public enum DurationUnit
{
    Seconds,
    Minutes,
    Hours,
    Liters,
}