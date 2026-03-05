using System.Text.Json.Serialization;

namespace Ecowitt.Controller.Model.Discovery;

public class Device
{
    [JsonPropertyName("identifiers")]
    public List<string> Identifiers { get; set; } = new List<string>();
    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; } = default!;
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
    [JsonPropertyName("hw_version")]
    public string HwVersion { get; set; } = default!;
    [JsonPropertyName("sw_version")]
    public string SwVersion { get; set; } = default!;
    [JsonPropertyName("via_device")]
    public string? ViaDevice { get; set; }
}