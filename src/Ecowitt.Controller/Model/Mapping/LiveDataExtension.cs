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
