namespace Ecowitt.Controller.Model;

public class Subdevice
{
    public int Id { get; set; }
    public string Nickname { get; set; }
    public string Devicename { get; set; }
    public SubdeviceModel Model { get; set; }
    public bool Availability { get; set; }
    public string GwIp { get; set; }
    public int Version { get; set; }
    public DateTime TimestampUtc { get; set; }
    public List<ISensor> Sensors { get; set; } = new List<ISensor>();
}

public enum SubdeviceModel
{
    Unknown = 0,
    WFC01 = 1,
    AC1100 = 2
}