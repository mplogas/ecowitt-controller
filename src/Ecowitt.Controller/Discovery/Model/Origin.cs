using Newtonsoft.Json;

namespace Ecowitt.Controller.Discovery.Model;

public class Origin
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("sw")]
    public string Sw { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }
}