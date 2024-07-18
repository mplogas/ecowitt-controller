namespace Ecowitt.Controller.Model;

public class SubdeviceApiData
{
    public int Id { get; set; }
    public int Model { get; set; }
    public int Version { get; set; }
    public int RfnetState { get; set; }
    public int Battery { get; set; }
    public int Signal { get; set; }
    public string GwIp { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string Payload { get; set; } = string.Empty;
}