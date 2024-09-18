using System.Text.Json.Serialization;

namespace Ecowitt.Controller.Discovery.Model;

public class Availability
{
    [JsonPropertyName("topic")]
    public string Topic { get; set; }
    [JsonPropertyName("payload_available")]
    public string PayloadAvailable { get; set; }
    [JsonPropertyName("payload_not_available")]
    public string PayloadUnavailable { get; set; }
    [JsonPropertyName("value_template")]
    public string ValueTemplate { get; set; }
}