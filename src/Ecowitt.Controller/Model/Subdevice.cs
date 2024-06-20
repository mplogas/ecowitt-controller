namespace Ecowitt.Controller.Model;

public class Subdevice
{
    public int Id { get; set; }
    public SubdeviceType Model { get; set; }
    public int Ver { get; set; }
    public int RfnetState { get; set; }
    public int Battery { get; set; }
    public int Signal { get; set; }
    public string GwIp { get; set; }
    public string Payload { get; set; } = string.Empty;
}

public enum SubdeviceType
{
    Unknown = 0,
    WFC01 = 1,
    AC1100 = 2
}