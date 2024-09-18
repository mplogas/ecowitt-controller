using System.Text.Json.Serialization;

namespace Ecowitt.Controller.Discovery.Model;

public class Config
{
    [JsonPropertyName("device")]
    public Device Device { get; set; }
    [JsonPropertyName("device_class")]
    public string? DeviceClass { get; set; }
    [JsonPropertyName("origin")]
    public Origin Origin { get; set; }  
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("retain")]
    public bool? Retain { get; set; }
    [JsonPropertyName("qos")]
    public int? Qos { get; set; }
    [JsonPropertyName("availability_topic")]
    public string AvailabilityTopic { get; set; }
    [JsonPropertyName("state_topic")]
    public string StateTopic { get; set; }
    [JsonPropertyName("command_topic")]
    public string? CommandTopic { get; set; }
    [JsonPropertyName("unique_id")]
    public string UniqueId { get; set; }
    [JsonPropertyName("object_id")]
    public string ObjectId { get; set; }
    [JsonPropertyName("unit_of_measurement")]
    public string? UnitOfMeasurement { get; set; }
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
    [JsonPropertyName("availability")]
    public List<Availability>? Availability { get; set; }
    [JsonPropertyName("availability_mode")]
    public AvailabilityMode? AvailabilityMode { get; set; }
    [JsonPropertyName("value_template")]
    public string? ValueTemplate { get; set; }
    [JsonPropertyName("entity_category")] 
    public string? SensorCategory { get; set; }

    [JsonPropertyName("payload_on")]
    public string? PayloadOn { get; set; }
    [JsonPropertyName("payload_off")]
    public string? PayloadOff { get; set; }
    [JsonPropertyName("state_on")]
    public string? StateOn { get; set; }
    [JsonPropertyName("state_off")]
    public string? StateOff { get; set; }
}   