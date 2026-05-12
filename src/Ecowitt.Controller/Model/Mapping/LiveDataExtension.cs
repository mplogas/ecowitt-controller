using System.Globalization;
using Ecowitt.Controller.Model.Message.Data;

namespace Ecowitt.Controller.Model.Mapping;

public static class LiveDataExtension
{
    public static Device Map(this GatewayLiveData data, bool isMetric = true, bool calculateValues = true)
    {
        var device = new Device
        {
            IpAddress = data.IpAddress,
            TimestampUtc = data.TimestampUtc,
            Sensors = new List<ISensor>()
        };

        if (data.Wh25 != null) MapWh25(data.Wh25, device.Sensors, isMetric);
        if (data.ChSoil != null) MapChSoil(data.ChSoil, device.Sensors, isMetric);
        if (data.ChTemp != null) MapChTemp(data.ChTemp, device.Sensors, isMetric);
        if (data.Lightning != null) MapLightning(data.Lightning, device.Sensors, isMetric);
        if (data.Co2 != null) MapCo2(data.Co2, device.Sensors, isMetric);
        if (data.PiezoRain != null) MapPiezoRain(data.PiezoRain, device.Sensors, isMetric);

        if (calculateValues) SensorBuilder.CalculateGatewayAddons(ref device, isMetric);

        return device;
    }

    private static void MapWh25(List<Wh25Reading> readings, List<ISensor> sensors, bool isMetric)
    {
        foreach (var r in readings)
        {
            var temp = SensorBuilder.BuildTemperatureSensor("tempinf", "Indoor Temperature", StripUnit(r.Intemp), isMetric, startMetric: true);
            if (temp != null) sensors.Add(temp);

            var humi = SensorBuilder.BuildHumiditySensor("humidityin", "Indoor Humidity", StripUnit(r.Inhumi));
            if (humi != null) sensors.Add(humi);

            var abs = SensorBuilder.BuildPressureSensor("baromabsin", "Absolute Pressure", StripUnit(r.Abs), isMetric, startMetric: true);
            if (abs != null) sensors.Add(abs);

            var rel = SensorBuilder.BuildPressureSensor("baromrelin", "Relative Pressure", StripUnit(r.Rel), isMetric, startMetric: true);
            if (rel != null) sensors.Add(rel);
        }
    }

    private static void MapChSoil(List<ChannelSoilReading> readings, List<ISensor> sensors, bool isMetric)
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

            // soilbatt — battery level 0-5
            var batt = SensorBuilder.BuildBatterySensor($"soilbatt{ch}", $"Soil Battery {ch}", StripUnit(r.Battery));
            if (batt != null) sensors.Add(batt);

            // soilbattvolt — new diagnostic voltage sensor
            var voltage = SensorBuilder.BuildVoltageSensor($"soilbattvolt{ch}", $"Soil Battery Voltage {ch}", StripUnit(r.Voltage), isDiag: true);
            if (voltage != null) sensors.Add(voltage);
        }
    }

    private static void MapChTemp(List<ChannelTempReading> readings, List<ISensor> sensors, bool isMetric)
    {
        foreach (var r in readings)
        {
            if (!int.TryParse(r.Channel, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ch)) continue;

            var temp = SensorBuilder.BuildTemperatureSensor($"temp{ch}f", $"Channel {ch} Temperature", StripUnit(r.Temp), isMetric, startMetric: true);
            if (temp != null) sensors.Add(temp);

            var batt = SensorBuilder.BuildBatterySensor($"batt{ch}", $"Channel {ch} Battery", StripUnit(r.Battery));
            if (batt != null) sensors.Add(batt);

            var voltage = SensorBuilder.BuildVoltageSensor($"battvolt{ch}", $"Channel {ch} Battery Voltage", StripUnit(r.Voltage), isDiag: true);
            if (voltage != null) sensors.Add(voltage);
        }
    }

    private static void MapLightning(List<LightningReading> readings, List<ISensor> sensors, bool isMetric)
    {
        foreach (var r in readings)
        {
            var distance = SensorBuilder.BuildDistanceSensor("lightning", "Lightning Distance", StripUnit(r.Distance), isMetric);
            if (distance != null) sensors.Add(distance);

            var count = SensorBuilder.BuildIntSensor("lightning_num", "Lightning Count", StripUnit(r.Count));
            if (count != null) sensors.Add(count);

            var battery = SensorBuilder.BuildBatterySensor("wh57batt", "Lightning Battery", StripUnit(r.Battery));
            if (battery != null) sensors.Add(battery);
        }
    }

    private static void MapCo2(List<Co2Reading> readings, List<ISensor> sensors, bool isMetric)
    {
        foreach (var r in readings)
        {
            // Temp: source is metric °C, but push convention via SensorBuilder switch assumes F. Direct call with startMetric:true.
            var temp = SensorBuilder.BuildTemperatureSensor("tf_co2", "CO2 Temperature", StripUnit(r.Temp), isMetric, startMetric: true);
            if (temp != null) sensors.Add(temp);

            // Remaining are unit-agnostic; route through public BuildSensor (uses existing AQIN switch arms).
            AddIfBuilt(sensors, "humi_co2", StripUnit(r.Humidity), isMetric);
            AddIfBuilt(sensors, "pm25_co2", StripUnit(r.Pm25), isMetric);
            AddIfBuilt(sensors, "pm25_24h_co2", StripUnit(r.Pm25_24H), isMetric);
            AddIfBuilt(sensors, "pm10_co2", StripUnit(r.Pm10), isMetric);
            AddIfBuilt(sensors, "pm10_24h_co2", StripUnit(r.Pm10_24H), isMetric);
            AddIfBuilt(sensors, "pm1_24h_co2", StripUnit(r.Pm1_24H), isMetric);
            AddIfBuilt(sensors, "pm4_24h_co2", StripUnit(r.Pm4_24H), isMetric);
            AddIfBuilt(sensors, "co2", StripUnit(r.Co2), isMetric);
            AddIfBuilt(sensors, "co2_24h", StripUnit(r.Co2_24H), isMetric);
            AddIfBuilt(sensors, "co2_batt", StripUnit(r.Battery), isMetric);
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

    private static void MapPiezoRain(List<PiezoRainItem> readings, List<ISensor> sensors, bool isMetric)
    {
        foreach (var r in readings)
        {
            if (!PiezoRainIdMap.TryGetValue(r.Id, out var propertyName)) continue;

            ISensor? sensor = propertyName == "rrain_piezo"
                ? SensorBuilder.BuildRainRateSensor(propertyName, RainAliasFor(propertyName), StripUnit(r.Val), isMetric, startMetric: true)
                : SensorBuilder.BuildRainSensor(propertyName, RainAliasFor(propertyName), StripUnit(r.Val), isMetric, startMetric: true);

            if (sensor != null) sensors.Add(sensor);
        }

        // WS90 telemetry on the LAST array element — Ecowitt firmware quirk.
        var last = readings.LastOrDefault();
        if (last == null) return;

        if (!string.IsNullOrEmpty(last.Battery))
        {
            var batt = SensorBuilder.BuildBatterySensor("ws90batt", "WS90 Battery", StripUnit(last.Battery));
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

    private static void AddIfBuilt(List<ISensor> sensors, string propertyName, string value, bool isMetric)
    {
        var sensor = SensorBuilder.BuildSensor(propertyName, value, isMetric);
        if (sensor != null) sensors.Add(sensor);
    }

    private static string StripUnit(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var trimmed = raw.Trim();
        // Strip trailing unit like " hPa", " mm", " m/s", etc.
        var space = trimmed.IndexOf(' ');
        if (space > 0) trimmed = trimmed.Substring(0, space);
        // Strip trailing percent sign
        if (trimmed.EndsWith("%")) trimmed = trimmed[..^1];
        return trimmed;
    }
}
