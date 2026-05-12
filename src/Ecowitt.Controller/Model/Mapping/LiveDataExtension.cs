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

        return device;
    }

    private static void MapWh25(List<Wh25Reading> readings, List<ISensor> sensors, bool isMetric)
    {
        foreach (var r in readings)
        {
            // Indoor temperature — livedata source is metric °C; construct directly to avoid double-conversion.
            var temp = BuildTemperatureFromCelsius("tempinf", "Indoor Temperature", StripUnit(r.Intemp), isMetric);
            if (temp != null) sensors.Add(temp);

            // Indoor humidity — % is unit-agnostic; delegate to SensorBuilder after stripping suffix.
            var humi = SensorBuilder.BuildSensor("humidityin", StripUnit(r.Inhumi), isMetric);
            if (humi != null) sensors.Add(humi);

            // Absolute pressure — livedata source is metric hPa; construct directly to avoid double-conversion.
            var abs = BuildPressureFromHectopascals("baromabsin", "Absolute Pressure", StripUnit(r.Abs), isMetric);
            if (abs != null) sensors.Add(abs);

            // Relative pressure — livedata source is metric hPa; construct directly to avoid double-conversion.
            var rel = BuildPressureFromHectopascals("baromrelin", "Relative Pressure", StripUnit(r.Rel), isMetric);
            if (rel != null) sensors.Add(rel);
        }
    }

    private static ISensor? BuildTemperatureFromCelsius(string propertyName, string alias, string celsiusStr, bool isMetric)
    {
        if (!double.TryParse(celsiusStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var celsius)) return null;
        var value = isMetric ? celsius : celsius * 9.0 / 5.0 + 32.0;
        var unit = isMetric ? "°C" : "F";
        return new Sensor(propertyName, alias, value, SensorDataType.Double, unit, SensorType.Temperature);
    }

    private static ISensor? BuildPressureFromHectopascals(string propertyName, string alias, string hpaStr, bool isMetric)
    {
        if (!double.TryParse(hpaStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var hpa)) return null;
        var value = isMetric ? hpa : hpa * 0.029530;  // hPa -> inHg
        var unit = isMetric ? "hPa" : "inHg";
        return new Sensor(propertyName, alias, value, SensorDataType.Double, unit, SensorType.Pressure);
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
