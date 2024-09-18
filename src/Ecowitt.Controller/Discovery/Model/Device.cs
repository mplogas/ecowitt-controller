using System.Text.Json.Serialization;

namespace Ecowitt.Controller.Discovery.Model;

public class Device
{
    [JsonPropertyName("identifiers")]
    public List<string> Identifiers { get; set; } = new List<string>();
    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; }
    [JsonPropertyName("model")]
    public string Model { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("hw_version")]
    public string HwVersion { get; set; }
    [JsonPropertyName("sw_version")]
    public string SwVersion { get; set; }
    [JsonPropertyName("via_device")]
    public string? ViaDevice { get; set; }
}