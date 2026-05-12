using System.Globalization;
using System.Text.RegularExpressions;
using Serilog;

namespace Ecowitt.Controller.Model.Mapping
{
    public partial class SensorBuilder
    {
        private static readonly HashSet<string> InvalidTokens = new(StringComparer.InvariantCultureIgnoreCase)
        {
            "-","--","na","n/a","nan","null","none",""
        };

        private static bool TryParseDouble(string? raw, out double value, string? prop = null)
        {
            value = default;
            if (string.IsNullOrWhiteSpace(raw)) return false;
            raw = raw.Trim();
            if (InvalidTokens.Contains(raw)) return false;
            if (raw.Contains(',') && !raw.Contains('.') && raw.IndexOf(',') == raw.LastIndexOf(','))
            {
                raw = raw.Replace(',', '.');
            }
            var ok = double.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
            if (!ok)
            {
                Log.Debug("Could not parse double value '{Raw}' for sensor property {Property}", raw, prop ?? "<unknown>");
            }
            return ok;
        }

        private static bool TryParseInt(string? raw, out int value, string? prop = null)
        {
            value = default;
            if (string.IsNullOrWhiteSpace(raw)) return false;
            raw = raw.Trim();
            if (InvalidTokens.Contains(raw)) return false;
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) return true;
            if (double.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var dbl))
            {
                if (dbl % 1 == 0 && dbl <= int.MaxValue && dbl >= int.MinValue)
                {
                    value = (int)dbl;
                    return true;
                }
            }
            Log.Debug("Could not parse int value '{Raw}' for sensor property {Property}", raw, prop ?? "<unknown>");
            return false;
        }

        private static bool IsInvalidString(string? raw) => string.IsNullOrWhiteSpace(raw) || InvalidTokens.Contains(raw.Trim());

        private static Sensor? BuildWaterFlowSensor(string propertyName, string alias, string propertyValue, bool isMetric = true)
        {
            return TryParseDouble(propertyValue, out var value, propertyName)
                ? new Sensor(propertyName, alias, isMetric ? value : L2G(value), SensorDataType.Double, isMetric ? "L/min" : "gal/min", SensorType.VolumeFlowRate)
                : null;
        }

        private static Sensor? BuildWaterConsumptionSensor(string propertyName, string alias, string propertyValue,
            bool isMetric = true, bool isTotal = false)
        {
            return TryParseDouble(propertyValue, out var value, propertyName)
                ? new Sensor(propertyName, alias, isMetric ? value : L2G(value), SensorDataType.Double, isMetric ? "L" : "gal", SensorType.Water, isTotal ? SensorState.TotalIncreasing : SensorState.Measurement)
                : null;
        }

        private static Sensor? BuildCurrentSensor(string propertyName, string alias, string propertyValue, bool isMilliAmp = false)
        {
            return TryParseInt(propertyValue, out var value, propertyName)
                ? new Sensor(propertyName, alias, value, SensorDataType.Integer, isMilliAmp ? "mA" : "A", SensorType.Current)
                : null;
        }   
        

        private static Sensor? BuildPowerSensor(string propertyName, string alias, string propertyValue)
        {
            return TryParseInt(propertyValue, out var value, propertyName)
                ? new Sensor(propertyName, alias, value, SensorDataType.Integer, "W", SensorType.Power)
                : null;
        }

        private static Sensor? BuildConsumptionSensor(string propertyName, string alias, string propertyValue, bool isTotal = false)
        {
            return TryParseInt(propertyValue, out var value, propertyName)
                ? new Sensor(propertyName, alias, value, SensorDataType.Integer, "Wh", SensorType.Energy, isTotal ? SensorState.TotalIncreasing : SensorState.Measurement)
                : null;
        }

        internal static Sensor? BuildDistanceSensor(string propertyName, string alias, string propertyValue,
            bool isMetric = true)
        {
            return TryParseDouble(propertyValue, out var value, propertyName)
                ? new Sensor(propertyName, alias, isMetric ? value : K2M(value), SensorDataType.Double, isMetric ? "km" : "miles", SensorType.Distance)
                : null;
        }

        private static Sensor? BuildBinarySensor(string propertyName, string alias, string propertyValue, bool isDiag = true)
        {
            if (propertyValue == null) return null;
            bool value;
            switch (propertyValue.Trim().ToLowerInvariant())
            {
                case "0":
                case "false":
                case "off":
                case "no":
                    value = false; break;
                case "1":
                case "true":
                case "on":
                case "yes":
                    value = true; break;
                default:
                    Log.Debug("Could not parse boolean value '{Raw}' for sensor property {Property}", propertyValue, propertyName);
                    return null;
            }
            return new Sensor(propertyName, alias, value, SensorDataType.Boolean, sensorType: SensorType.None, sensorClass: SensorClass.BinarySensor, sensorCategory: isDiag ? SensorCategory.Diagnostic : SensorCategory.Config);
        }

        internal static Sensor? BuildBatterySensor(string propertyName, string alias, string propertyValue, bool withMultiplier = false )
        {
            if (!TryParseInt(propertyValue, out var value, propertyName)) return null;
            if (withMultiplier)
            {
                value *= 20; // multiplier
            }
            if (value > 100) value = 100;
            if (value < 0) value = 0;
            return new Sensor(propertyName, alias, value, SensorDataType.Integer, "%", SensorType.Battery, sensorCategory: SensorCategory.Diagnostic);
        }

        internal static Sensor? BuildPPMSensor(string propertyName, string alias, string propertyValue, SensorType sensorType, bool isTotal = false)
        {
            return TryParseInt(propertyValue, out var value, propertyName)
                ? new Sensor(propertyName, alias, value, SensorDataType.Integer, "ppm", sensorType, isTotal ? SensorState.Total : SensorState.Measurement)
                : null;
        }

        internal static Sensor? BuildParticleSensor(string propertyName, string alias, string propertyValue, SensorType sensorType, bool isMetric = true, bool isTotal = false)
        {
            return TryParseDouble(propertyValue, out var value, propertyName)
                ? new Sensor(propertyName, alias, value, SensorDataType.Double, "µg/m³", sensorType, isTotal ? SensorState.Total : SensorState.Measurement)
                : null;
        }

        internal static Sensor? BuildVoltageSensor(string propertyName, string alias, string propertyValue, bool isDiag = false)
        {
            return TryParseDouble(propertyValue, out var value, propertyName)
                ? new Sensor(propertyName, alias, value, SensorDataType.Double, "V", SensorType.Voltage, sensorCategory: isDiag ? SensorCategory.Diagnostic : SensorCategory.Config)
                : null;
        }

        internal static Sensor? BuildRainRateSensor(string propertyName, string alias, string propertyValue, bool isMetric, bool startMetric = false)
        {
            if (!TryParseDouble(propertyValue, out var value, propertyName)) return null;
            if (startMetric != isMetric) value = startMetric ? M2I(value) : I2M(value);
            return new Sensor(propertyName, alias, value, SensorDataType.Double, isMetric ? "mm/h" : "in/h", SensorType.PrecipitationIntensity);
        }

        internal static Sensor? BuildRainSensor(string propertyName, string alias, string propertyValue, bool isMetric, bool startMetric = false)
        {
            if (!TryParseDouble(propertyValue, out var value, propertyName)) return null;
            if (startMetric != isMetric) value = startMetric ? M2I(value) : I2M(value);
            return new Sensor(propertyName, alias, value, SensorDataType.Double, isMetric ? "mm" : "in", SensorType.Precipitation);
        }

        internal enum WindSpeedSourceUnit { Mph, MetersPerSecond }

        internal static Sensor? BuildWindSpeedSensor(string propertyName, string alias, string propertyValue, bool isMetric, WindSpeedSourceUnit sourceUnit = WindSpeedSourceUnit.Mph)
        {
            if (!TryParseDouble(propertyValue, out var value, propertyName)) return null;
            double output = sourceUnit switch
            {
                WindSpeedSourceUnit.MetersPerSecond when isMetric => MS2K(value),
                WindSpeedSourceUnit.MetersPerSecond when !isMetric => MS2P(value),
                WindSpeedSourceUnit.Mph when isMetric => M2K(value),
                WindSpeedSourceUnit.Mph when !isMetric => value,
                _ => value
            };
            return new Sensor(propertyName, alias, output, SensorDataType.Double, isMetric ? "km/h" : "mph", SensorType.WindSpeed);
        }

        internal static Sensor? BuildPressureSensor(string propertyName, string alias, string propertyValue, bool isMetric, bool startMetric = false)
        {
            if (!TryParseDouble(propertyValue, out var value, propertyName)) return null;
            var unit = isMetric ? "hPa" : "inHg";
            if (startMetric != isMetric)
            {
                value = startMetric ? HP2IM(value) : IM2HP(value);
            }
            return new Sensor(propertyName, alias, value, SensorDataType.Double, unit, SensorType.Pressure);
        }

        internal static Sensor? BuildHumiditySensor(string propertyName, string alias, string propertyValue)
        {
            return TryParseDouble(propertyValue, out var value, propertyName)
                ? new Sensor(propertyName, alias, value, SensorDataType.Double, "%", SensorType.Humidity)
                : null;
        }

        internal static Sensor? BuildTemperatureSensor(string propertyName, string alias, string propertyValue, bool isMetric, bool startMetric = false)
        {
            if (!TryParseDouble(propertyValue, out var value, propertyName)) return null;
            var unit = isMetric ? "°C" : "F";
            if (startMetric != isMetric)
            {
                value = startMetric ? C2F(value) : F2C(value);
            }
            return new Sensor(propertyName, alias, value, SensorDataType.Double, unit, SensorType.Temperature);
        }

        internal static Sensor? BuildDoubleSensor(string propertyName, string alias, string propertyValue, string unit = "", SensorType type = SensorType.None, bool isDiag = false)
        {
            return TryParseDouble(propertyValue, out var value, propertyName)
                ? new Sensor(propertyName, alias, value, SensorDataType.Double, unit, type, sensorCategory: isDiag ? SensorCategory.Diagnostic : SensorCategory.Config)
                : null;
        }

        private static Sensor? BuildDateTimeSensor(string propertyName, string alias, string propertyValue)
        {
            if (long.TryParse(propertyValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ts))
            {
                return new Sensor(propertyName, alias, DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime, SensorDataType.DateTime);
            }
            if (DateTime.TryParse(propertyValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
            {
                return new Sensor(propertyName, alias, DateTime.SpecifyKind(dt, DateTimeKind.Utc), SensorDataType.DateTime);
            }
            Log.Debug("Could not parse datetime value '{Raw}' for sensor property {Property}", propertyValue, propertyName);
            return null;
        }

        internal static Sensor? BuildIntSensor(string propertyName, string alias, string propertyValue, string unit = "", SensorType type = SensorType.None, bool isDiag = false)
        {
            return TryParseInt(propertyValue, out var value, propertyName)
                ? new Sensor(propertyName, alias, value, SensorDataType.Integer, unit, type, sensorCategory: isDiag ? SensorCategory.Diagnostic : SensorCategory.Config)
                : null;
        }

        private static Sensor? BuildStringSensor(string propertyName, string alias, string propertyValue, bool isDiag = false)
        {
            if (IsInvalidString(propertyValue)) return null;
            return new Sensor(propertyName, alias, propertyValue, SensorDataType.String, string.Empty, sensorCategory: isDiag ? SensorCategory.Diagnostic : SensorCategory.Config);
        }

        private static int GetNumber(string propertyName)
        {
            const string pattern = @"(\d+)$";
            var m = Regex.Match(propertyName, pattern, RegexOptions.Compiled);
            return m.Success ? int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : -1;
        }

        private static double K2M(double result) => result * 0.621371;
        private static double IM2HP(double im) => im * 33.86388;
        private static double HP2IM(double hpa) => hpa / 33.86388;
        private static double F2C(double fahrenheit) => (fahrenheit - 32) * 5 / 9;
        private static double C2F(double celsius) => celsius * 9 / 5 + 32;
        private static double M2K(double mph) => mph * 1.60934;
        private static double I2M(double inches) => inches * 25.4;
        private static double M2I(double mm) => mm / 25.4;
        private static double MS2K(double ms) => ms * 3.6;
        private static double MS2P(double ms) => ms * 2.23694;
        private static double L2G(double liters) => liters * 0.264172;
    }
}
