using System.Text.Json.Serialization;

namespace Ecowitt.Controller.Model.Discovery;

public class Availability
{
    [JsonPropertyName("topic")]
    public string Topic { get; set; } = default!;
    [JsonPropertyName("payload_available")]
    public string PayloadAvailable { get; set; } = default!;
    [JsonPropertyName("payload_not_available")]
    public string PayloadUnavailable { get; set; } = default!;
    [JsonPropertyName("value_template")]
    public string ValueTemplate { get; set; } = default!;
}