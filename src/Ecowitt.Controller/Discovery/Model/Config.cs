using Newtonsoft.Json;

namespace Ecowitt.Controller.Discovery.Model;

public class Config
{
    [JsonProperty("device")]
    public Device Device { get; set; }
    [JsonProperty("device_class")]
    public string? DeviceClass { get; set; }
    [JsonProperty("origin")]
    public Origin Origin { get; set; }  
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("retain")]
    public bool? Retain { get; set; }
    [JsonProperty("qos")]
    public int? Qos { get; set; }
    [JsonProperty("availability_topic")]
    public string AvailabilityTopic { get; set; }
    [JsonProperty("state_topic")]
    public string StateTopic { get; set; }
    [JsonProperty("command_topic")]
    public string? CommandTopic { get; set; }
    [JsonProperty("unique_id")]
    public string UniqueId { get; set; }
    [JsonProperty("object_id")]
    public string ObjectId { get; set; }
    [JsonProperty("unit_of_measurement")]
    public string? UnitOfMeasurement { get; set; }
    [JsonProperty("icon")]
    public string? Icon { get; set; }
    [JsonProperty("availability")]
    public List<Availability>? Availability { get; set; }
    [JsonProperty("availability_mode")]
    public AvailabilityMode? AvailabilityMode { get; set; }
    [JsonProperty("value_template")]
    public string ValueTemplate { get; set; }
}   