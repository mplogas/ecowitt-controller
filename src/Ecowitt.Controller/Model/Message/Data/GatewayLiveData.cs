using System.Text.Json.Serialization;

namespace Ecowitt.Controller.Model.Message.Data;

public class GatewayLiveData
{
    public string IpAddress { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }

    [JsonPropertyName("common_list")]
    public List<CommonListItem>? CommonList { get; set; }

    [JsonPropertyName("piezoRain")]
    public List<PiezoRainItem>? PiezoRain { get; set; }

    [JsonPropertyName("wh25")]
    public List<Wh25Reading>? Wh25 { get; set; }

    [JsonPropertyName("lightning")]
    public List<LightningReading>? Lightning { get; set; }

    [JsonPropertyName("co2")]
    public List<Co2Reading>? Co2 { get; set; }

    [JsonPropertyName("ch_soil")]
    public List<ChannelSoilReading>? ChSoil { get; set; }

    [JsonPropertyName("ch_temp")]
    public List<ChannelTempReading>? ChTemp { get; set; }
}

public class CommonListItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("val")]
    public string Val { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }
}

public class PiezoRainItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("val")]
    public string Val { get; set; } = string.Empty;

    [JsonPropertyName("battery")]
    public string? Battery { get; set; }

    [JsonPropertyName("voltage")]
    public string? Voltage { get; set; }

    [JsonPropertyName("ws90cap_volt")]
    public string? Ws90CapVolt { get; set; }

    [JsonPropertyName("ws90_ver")]
    public string? Ws90Version { get; set; }
}

public class Wh25Reading
{
    [JsonPropertyName("intemp")]
    public string Intemp { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("inhumi")]
    public string Inhumi { get; set; } = string.Empty;

    [JsonPropertyName("abs")]
    public string Abs { get; set; } = string.Empty;

    [JsonPropertyName("rel")]
    public string Rel { get; set; } = string.Empty;
}

public class LightningReading
{
    [JsonPropertyName("distance")]
    public string Distance { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public string Count { get; set; } = string.Empty;

    [JsonPropertyName("battery")]
    public string Battery { get; set; } = string.Empty;
}

public class Co2Reading
{
    [JsonPropertyName("temp")]
    public string Temp { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("humidity")]
    public string Humidity { get; set; } = string.Empty;

    [JsonPropertyName("PM25")]
    public string Pm25 { get; set; } = string.Empty;

    [JsonPropertyName("PM25_RealAQI")]
    public string Pm25RealAqi { get; set; } = string.Empty;

    [JsonPropertyName("PM25_24HAQI")]
    public string Pm25_24HAqi { get; set; } = string.Empty;

    [JsonPropertyName("PM25_24H")]
    public string Pm25_24H { get; set; } = string.Empty;

    [JsonPropertyName("PM10")]
    public string Pm10 { get; set; } = string.Empty;

    [JsonPropertyName("PM10_RealAQI")]
    public string Pm10RealAqi { get; set; } = string.Empty;

    [JsonPropertyName("PM10_24HAQI")]
    public string Pm10_24HAqi { get; set; } = string.Empty;

    [JsonPropertyName("PM10_24H")]
    public string Pm10_24H { get; set; } = string.Empty;

    [JsonPropertyName("PM1_24H")]
    public string Pm1_24H { get; set; } = string.Empty;

    [JsonPropertyName("PM4_24H")]
    public string Pm4_24H { get; set; } = string.Empty;

    [JsonPropertyName("CO2")]
    public string Co2 { get; set; } = string.Empty;

    [JsonPropertyName("CO2_24H")]
    public string Co2_24H { get; set; } = string.Empty;

    [JsonPropertyName("battery")]
    public string Battery { get; set; } = string.Empty;
}

public class ChannelSoilReading
{
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("battery")]
    public string Battery { get; set; } = string.Empty;

    [JsonPropertyName("voltage")]
    public string Voltage { get; set; } = string.Empty;

    [JsonPropertyName("humidity")]
    public string Humidity { get; set; } = string.Empty;
}

public class ChannelTempReading
{
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("temp")]
    public string Temp { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("battery")]
    public string Battery { get; set; } = string.Empty;

    [JsonPropertyName("voltage")]
    public string Voltage { get; set; } = string.Empty;
}
