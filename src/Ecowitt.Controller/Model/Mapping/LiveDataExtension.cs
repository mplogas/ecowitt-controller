using System.Globalization;
using Ecowitt.Controller.Model.Message.Data;

namespace Ecowitt.Controller.Model.Mapping;

public static class LiveDataExtension
{
    // isMetric and calculateValues are kept for API parity with ApiDataExtension.Map but have
    // no effect here: livedata sensors carry the unit the gateway returned, and the gateway
    // already provides derived values (dewpoint via 0x03, feelslike via id 4). Recomputing them
    // with CalculateGatewayAddons risks producing values in the wrong unit because individual
    // sensors may differ from the controller's isMetric setting under passthrough.
    public static Device Map(this GatewayLiveData data, bool isMetric = true, bool calculateValues = true)
    {
        _ = isMetric;
        _ = calculateValues;

        var device = new Device
        {
            IpAddress = data.IpAddress,
            TimestampUtc = data.TimestampUtc,
            Sensors = new List<ISensor>()
        };

        if (data.Wh25 != null) MapWh25(data.Wh25, device.Sensors);
        if (data.ChSoil != null) MapChSoil(data.ChSoil, device.Sensors);
        if (data.ChTemp != null) MapChTemp(data.ChTemp, device.Sensors);
        if (data.Lightning != null) MapLightning(data.Lightning, device.Sensors);
        if (data.Co2 != null) MapCo2(data.Co2, device.Sensors);
        if (data.PiezoRain != null) MapPiezoRain(data.PiezoRain, device.Sensors);
        if (data.CommonList != null) MapCommonList(data.CommonList, device.Sensors);

        return device;
    }

    private static void MapWh25(List<Wh25Reading> readings, List<ISensor> sensors)
    {
        foreach (var r in readings)
        {
            var tempUnit = NormalizeTemperatureUnit(r.Unit);
            var temp = SensorBuilder.BuildDoubleSensor("tempinf", "Indoor Temperature", r.Intemp, tempUnit, SensorType.Temperature);
            if (temp != null) sensors.Add(temp);

            var humi = SensorBuilder.BuildDoubleSensor("humidityin", "Indoor Humidity", StripUnit(r.Inhumi), "%", SensorType.Humidity);
            if (humi != null) sensors.Add(humi);

            var (absVal, absUnit) = SplitValueAndUnit(r.Abs);
            var abs = SensorBuilder.BuildDoubleSensor("baromabsin", "Absolute Pressure", absVal, absUnit, SensorType.Pressure);
            if (abs != null) sensors.Add(abs);

            var (relVal, relUnit) = SplitValueAndUnit(r.Rel);
            var rel = SensorBuilder.BuildDoubleSensor("baromrelin", "Relative Pressure", relVal, relUnit, SensorType.Pressure);
            if (rel != null) sensors.Add(rel);
        }
    }

    private static void MapChSoil(List<ChannelSoilReading> readings, List<ISensor> sensors)
    {
        foreach (var r in readings)
        {
            if (!int.TryParse(r.Channel, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ch)) continue;

            // soilmoisture — skip the "--" sentinel
            var rawHumi = StripUnit(r.Humidity);
            if (!string.IsNullOrEmpty(rawHumi) && rawHumi != "--")
            {
                var moisture = SensorBuilder.BuildHumiditySensor($"soilmoisture{ch}", $"Soil Moisture {ch}", rawHumi);
                if (moisture != null) sensors.Add(moisture);
            }

            // soilbatt is voltage in the push payload (gateway sends "1.4V"); livedata gives both a
            // 0-5 level and the actual voltage. The voltage is the authoritative reading and matches
            // push semantics — emit only that, named soilbatt{ch} to stay continuous across modes.
            var voltage = SensorBuilder.BuildVoltageSensor($"soilbatt{ch}", $"Soil Battery {ch}", StripUnit(r.Voltage), isDiag: true);
            if (voltage != null) sensors.Add(voltage);
        }
    }

    private static void MapChTemp(List<ChannelTempReading> readings, List<ISensor> sensors)
    {
        foreach (var r in readings)
        {
            if (!int.TryParse(r.Channel, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ch)) continue;

            var tempUnit = NormalizeTemperatureUnit(r.Unit);
            var temp = SensorBuilder.BuildDoubleSensor($"temp{ch}f", $"Channel {ch} Temperature", r.Temp, tempUnit, SensorType.Temperature);
            if (temp != null) sensors.Add(temp);

            var batt = SensorBuilder.BuildBatterySensor($"batt{ch}", $"Channel {ch} Battery", StripUnit(r.Battery));
            if (batt != null) sensors.Add(batt);

            var voltage = SensorBuilder.BuildVoltageSensor($"battvolt{ch}", $"Channel {ch} Battery Voltage", StripUnit(r.Voltage), isDiag: true);
            if (voltage != null) sensors.Add(voltage);
        }
    }

    private static void MapLightning(List<LightningReading> readings, List<ISensor> sensors)
    {
        foreach (var r in readings)
        {
            var (distVal, distUnit) = SplitValueAndUnit(r.Distance);
            var distance = SensorBuilder.BuildDoubleSensor("lightning", "Lightning Distance", distVal, distUnit, SensorType.Distance);
            if (distance != null) sensors.Add(distance);

            var count = SensorBuilder.BuildIntSensor("lightning_num", "Lightning Count", StripUnit(r.Count));
            if (count != null) sensors.Add(count);

            var battery = SensorBuilder.BuildBatterySensor("wh57batt", "Lightning Battery", StripUnit(r.Battery), withMultiplier: true);
            if (battery != null) sensors.Add(battery);
        }
    }

    private static void MapCo2(List<Co2Reading> readings, List<ISensor> sensors)
    {
        foreach (var r in readings)
        {
            var tempUnit = NormalizeTemperatureUnit(r.Unit);
            var temp = SensorBuilder.BuildDoubleSensor("tf_co2", "CO2 Temperature", r.Temp, tempUnit, SensorType.Temperature);
            if (temp != null) sensors.Add(temp);

            AddDoubleIfValid(sensors, "humi_co2",       "CO2 Humidity",    StripUnit(r.Humidity),  "%",      SensorType.Humidity);
            AddDoubleIfValid(sensors, "pm25_co2",       "PM2.5 (CO2)",     StripUnit(r.Pm25),      "µg/m³", SensorType.Pm25);
            AddDoubleIfValid(sensors, "pm25_24h_co2",   "PM2.5 24h (CO2)", StripUnit(r.Pm25_24H),  "µg/m³", SensorType.Pm25);
            AddDoubleIfValid(sensors, "pm10_co2",       "PM10 (CO2)",      StripUnit(r.Pm10),      "µg/m³", SensorType.Pm10);
            AddDoubleIfValid(sensors, "pm10_24h_co2",   "PM10 24h (CO2)",  StripUnit(r.Pm10_24H),  "µg/m³", SensorType.Pm10);
            AddDoubleIfValid(sensors, "pm1_24h_co2",    "PM1 24h (CO2)",   StripUnit(r.Pm1_24H),   "µg/m³", SensorType.Pm1);
            AddDoubleIfValid(sensors, "pm4_24h_co2",    "PM4 24h (CO2)",   StripUnit(r.Pm4_24H),   "µg/m³", SensorType.None);
            AddDoubleIfValid(sensors, "co2",            "CO2",             StripUnit(r.Co2),       "ppm",   SensorType.CarbonDioxide);
            AddDoubleIfValid(sensors, "co2_24h",        "CO2 24h",         StripUnit(r.Co2_24H),   "ppm",   SensorType.CarbonDioxide);

            var batt = SensorBuilder.BuildBatterySensor("co2_batt", "CO2 Battery", StripUnit(r.Battery), withMultiplier: true);
            if (batt != null) sensors.Add(batt);
        }
    }

    // Map from livedata piezoRain ids to push-payload property names.
    private static readonly Dictionary<string, string> PiezoRainIdMap = new()
    {
        { "0x0D", "erain_piezo" },   // event
        { "0x0E", "rrain_piezo" },   // rate
        { "0x7C", "drain_piezo" },   // daily
        { "0x10", "hrain_piezo" },   // hourly
        { "0x11", "wrain_piezo" },   // weekly
        { "0x12", "mrain_piezo" },   // monthly
        { "0x13", "yrain_piezo" }    // yearly
    };

    private static void MapPiezoRain(List<PiezoRainItem> readings, List<ISensor> sensors)
    {
        foreach (var r in readings)
        {
            if (!PiezoRainIdMap.TryGetValue(r.Id, out var propertyName)) continue;

            var (val, unit) = SplitValueAndUnit(r.Val);
            var type = propertyName == "rrain_piezo" ? SensorType.PrecipitationIntensity : SensorType.Precipitation;
            var sensor = SensorBuilder.BuildDoubleSensor(propertyName, RainAliasFor(propertyName), val, unit, type);
            if (sensor != null) sensors.Add(sensor);
        }

        // WS90 telemetry on the LAST array element — Ecowitt firmware quirk.
        var last = readings.LastOrDefault();
        if (last == null) return;

        if (!string.IsNullOrEmpty(last.Battery))
        {
            var batt = SensorBuilder.BuildBatterySensor("ws90batt", "WS90 Battery", StripUnit(last.Battery), withMultiplier: true);
            if (batt != null) sensors.Add(batt);
        }
        if (!string.IsNullOrEmpty(last.Voltage))
        {
            var volt = SensorBuilder.BuildVoltageSensor("ws90battvolt", "WS90 Battery Voltage", StripUnit(last.Voltage), isDiag: true);
            if (volt != null) sensors.Add(volt);
        }
        if (!string.IsNullOrEmpty(last.Ws90CapVolt))
        {
            var cap = SensorBuilder.BuildVoltageSensor("ws90cap_volt", "WS90 Capacitor Voltage", StripUnit(last.Ws90CapVolt), isDiag: true);
            if (cap != null) sensors.Add(cap);
        }
    }

    private static readonly Dictionary<string, string> CommonListIdMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "0x02", "tempf" },           // outdoor temperature
        { "0x03", "dewpoint" },        // dew point
        { "0x07", "humidity" },        // outdoor humidity
        { "0x0A", "winddir" },         // wind direction
        { "0x0B", "windspeedmph" },    // wind speed
        { "0x0C", "windgustmph" },     // wind gust
        { "0x19", "maxdailygust" },    // max daily gust
        { "0x15", "solarradiation" },  // solar radiation
        { "0x17", "uv" },              // UV
        { "0x6D", "windrun" },         // wind run
        { "4",    "feelslike" },       // feels-like (decimal id)
        { "5",    "vpd" }              // VPD (decimal id)
    };

    private static void MapCommonList(List<CommonListItem> readings, List<ISensor> sensors)
    {
        foreach (var r in readings)
        {
            if (!CommonListIdMap.TryGetValue(r.Id, out var propertyName))
            {
                Serilog.Log.Debug("livedata common_list: unrecognised sensor id {Id} (val={Val} unit={Unit})", r.Id, r.Val, r.Unit ?? "<none>");
                continue;
            }

            // Temperature entries carry value as bare number and unit in the separate `unit` field.
            // All other entries embed the unit suffix in the `val` string (e.g. "0.6 m/s", "0.431 kPa").
            var (rawValue, embeddedUnit) = SplitValueAndUnit(r.Val);
            var unit = string.IsNullOrEmpty(embeddedUnit) && r.Unit != null ? r.Unit : embeddedUnit;

            var sensor = BuildCommonListSensor(propertyName, rawValue, unit);
            if (sensor != null) sensors.Add(sensor);
        }
    }

    private static ISensor? BuildCommonListSensor(string propertyName, string rawValue, string unit)
    {
        return propertyName switch
        {
            "tempf"          => SensorBuilder.BuildDoubleSensor("tempf",          "Outdoor Temperature",    rawValue, NormalizeTemperatureUnit(unit), SensorType.Temperature),
            "dewpoint"       => SensorBuilder.BuildDoubleSensor("dewpoint",       "Dew Point",              rawValue, NormalizeTemperatureUnit(unit), SensorType.Temperature),
            "feelslike"      => SensorBuilder.BuildDoubleSensor("feelslike",      "Feels Like",             rawValue, NormalizeTemperatureUnit(unit), SensorType.Temperature),
            "humidity"       => SensorBuilder.BuildDoubleSensor("humidity",       "Outdoor Humidity",       rawValue, "%", SensorType.Humidity),
            "winddir"        => SensorBuilder.BuildIntSensor   ("winddir",        "Wind Direction",         rawValue, "°"),
            "windspeedmph"   => SensorBuilder.BuildDoubleSensor("windspeedmph",   "Wind Speed",             rawValue, unit, SensorType.WindSpeed),
            "windgustmph"    => SensorBuilder.BuildDoubleSensor("windgustmph",    "Wind Gust",              rawValue, unit, SensorType.WindSpeed),
            "maxdailygust"   => SensorBuilder.BuildDoubleSensor("maxdailygust",   "Max Daily Gust",         rawValue, unit, SensorType.WindSpeed),
            "solarradiation" => SensorBuilder.BuildDoubleSensor("solarradiation", "Solar Radiation",        rawValue, "W/m²", SensorType.Irradiance),
            "uv"             => SensorBuilder.BuildIntSensor   ("uv",             "UV Index",               rawValue),
            // windrun: gateway sends unitless value (e.g. "81"); no way to determine km vs mi from the
            // response. Drop the device_class so HA doesn't warn about a missing unit for distance.
            "windrun"        => SensorBuilder.BuildDoubleSensor("windrun",        "Wind Run",               rawValue, unit, SensorType.None),
            "vpd"            => SensorBuilder.BuildDoubleSensor("vpd",            "Vapor Pressure Deficit", rawValue, unit, SensorType.Pressure),
            _                => null
        };
    }

    private static string RainAliasFor(string propertyName) => propertyName switch
    {
        "erain_piezo" => "Event Rain",
        "rrain_piezo" => "Rain Rate",
        "drain_piezo" => "Daily Rain",
        "hrain_piezo" => "Hourly Rain",
        "wrain_piezo" => "Weekly Rain",
        "mrain_piezo" => "Monthly Rain",
        "yrain_piezo" => "Yearly Rain",
        _ => propertyName
    };

    // Splits a livedata value-with-unit string like "21.60 km/h" or "49%" into (value, unit).
    private static (string Value, string Unit) SplitValueAndUnit(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return (string.Empty, string.Empty);
        var trimmed = raw.Trim();
        var space = trimmed.IndexOf(' ');
        if (space > 0) return (trimmed.Substring(0, space), trimmed.Substring(space + 1).Trim());
        if (trimmed.EndsWith("%")) return (trimmed[..^1], "%");
        return (trimmed, string.Empty);
    }

    private static string StripUnit(string raw) => SplitValueAndUnit(raw).Value;

    // HA's device_class:temperature requires "°C"/"°F"/"K". The Ecowitt gateway sends bare "C" or "F"
    // in the separate `unit` field. Prepend the degree symbol; passthrough anything else.
    private static string NormalizeTemperatureUnit(string unit) => unit switch
    {
        "C" => "°C",
        "F" => "°F",
        _ => unit
    };

    private static void AddDoubleIfValid(List<ISensor> sensors, string propertyName, string alias, string value, string unit, SensorType type)
    {
        if (string.IsNullOrEmpty(value)) return;
        // skip the "--.-" / "--" sentinels Ecowitt uses for "no reading"
        if (value.Replace(".", "").Replace("-", "").Length == 0) return;
        var sensor = SensorBuilder.BuildDoubleSensor(propertyName, alias, value, unit, type);
        if (sensor != null) sensors.Add(sensor);
    }
}
