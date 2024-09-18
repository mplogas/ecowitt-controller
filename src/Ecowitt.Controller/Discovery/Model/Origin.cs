using System.Text.Json.Serialization;

namespace Ecowitt.Controller.Discovery.Model;

public class Origin
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("sw")]
    public string Sw { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
}