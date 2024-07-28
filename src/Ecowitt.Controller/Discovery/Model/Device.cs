using Newtonsoft.Json;

namespace Ecowitt.Controller.Discovery.Model;

public class Device
{
    [JsonProperty("identifiers")]
    public List<string> Identifiers { get; set; } = new List<string>();
    [JsonProperty("manufacturer")]
    public string Manufacturer { get; set; }
    [JsonProperty("model")]
    public string Model { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("hw_version")]
    public string HwVersion { get; set; }
    [JsonProperty("sw_version")]
    public string SwVersion { get; set; }
    [JsonProperty("via_device")]
    public string? ViaDevice { get; set; }
}