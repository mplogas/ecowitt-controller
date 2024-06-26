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
    Start = 0,
    Stop = 1
}

public enum DurationUnit
{
    Seconds = 0,
    Minutes = 1,
    Hours = 2,
    Liters = 3
}