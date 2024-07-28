using Newtonsoft.Json;

namespace Ecowitt.Controller.Discovery.Model;

public class Availability
{
    [JsonProperty("topic")]
    public string Topic { get; set; }
    [JsonProperty("payload_available")]
    public string PayloadAvailable { get; set; }
    [JsonProperty("payload_not_available")]
    public string PayloadUnavailable { get; set; }
    [JsonProperty("value_template")]
    public string ValueTemplate { get; set; }
}