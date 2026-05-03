using System.Text.Json.Serialization;

namespace Ecowitt.Controller.Model.Discovery;

public class Config
{
    [JsonPropertyName("device")]
    public Device Device { get; set; } = default!;
    [JsonPropertyName("device_class")]
    public string? DeviceClass { get; set; }
    [JsonPropertyName("origin")]
    public Origin Origin { get; set; } = default!;
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
    [JsonPropertyName("retain")]
    public bool? Retain { get; set; }
    [JsonPropertyName("qos")]
    public int? Qos { get; set; }
    [JsonPropertyName("availability_topic")]
    public string AvailabilityTopic { get; set; } = default!;
    [JsonPropertyName("state_topic")]
    public string StateTopic { get; set; } = default!;
    [JsonPropertyName("command_topic")]
    public string? CommandTopic { get; set; }
    [JsonPropertyName("unique_id")]
    public string UniqueId { get; set; } = default!;
    [JsonPropertyName("default_entity_id")]
    public string DefaultEntityId { get; set; } = default!;
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
    [JsonPropertyName("suggested_display_precision")]
    public int? SuggestedDisplayPrecision { get; set; }

    [JsonPropertyName("payload_on")]
    public string? PayloadOn { get; set; }
    [JsonPropertyName("payload_off")]
    public string? PayloadOff { get; set; }
    [JsonPropertyName("state_on")]
    public string? StateOn { get; set; }
    [JsonPropertyName("state_off")]
    public string? StateOff { get; set; }
}   