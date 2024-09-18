using System.Data.SqlTypes;
using Ecowitt.Controller.Model;
using System.Text.RegularExpressions;

namespace Ecowitt.Controller.Mapping
{
    public partial class SensorBuilder
    {
        private static Sensor<double>? BuildWaterFlowSensor(string propertyName, string alias, string propertyValue, bool isMetric = true)
        {
            return double.TryParse(propertyValue, out var value)
                ? new Sensor<double>(propertyName, alias, isMetric ? L2G(value) : value, isMetric ? "L/min" : "gal/min", SensorType.VolumeFlowRate)
                : null;
        }

        private static Sensor<double>? BuildWaterConsumptionSensor(string propertyName, string alias, string propertyValue,
            bool isMetric = true, bool isTotal = false)
        {
            return double.TryParse(propertyValue, out var value)
                ? new Sensor<double>(propertyName, alias, isMetric ? L2G(value) : value, isMetric ? "L" : "gal", SensorType.Water, isTotal ? SensorState.TotalIncreasing : SensorState.Measurement)
                : null;
        }

        private static Sensor<int>? BuildCurrentSensor(string propertyName, string alias, string propertyValue)
        {
            return int.TryParse(propertyValue, out var value)
                ? new Sensor<int>(propertyName, alias, value, "A", SensorType.Current)
                : null;
        }

        private static Sensor<int>? BuildPowerSensor(string propertyName, string alias, string propertyValue)
        {
            return int.TryParse(propertyValue, out var value)
                ? new Sensor<int>(propertyName, alias, value, "W", SensorType.Power)
                : null;
        }

        private static Sensor<int>? BuildConsumptionSensor(string propertyName, string alias, string propertyValue, bool isTotal = false)
        {
            return int.TryParse(propertyValue, out var value)
                ? new Sensor<int>(propertyName, alias, value, "Wh", SensorType.Energy, isTotal ? SensorState.TotalIncreasing : SensorState.Measurement)
                : null;
        }

        private static Sensor<double>? BuildDistanceSensor(string propertyName, string alias, string propertyValue,
            bool isMetric = true)
        {
            return double.TryParse(propertyValue, out var value)
                ? new Sensor<double>(propertyName, alias, isMetric ? value : K2M(value), isMetric ? "km" : "miles", SensorType.Distance)
                : null;
        }

        private static Sensor<bool>? BuildBinarySensor(string propertyName, string alias, string propertyValue, bool isDiag = true)
        {
            bool value;
            switch (propertyValue)
            {
                case "0":
                case "false":
                    value = false;
                    break;
                case "1":
                case "true":
                    value = true;
                    break;
                default:
                    return null;
            }

            return new Sensor<bool>(propertyName, alias, value, sensorClass: SensorClass.BinarySensor, sensorCategory: isDiag ? SensorCategory.Diagnostic : SensorCategory.Config);
        }

        private static Sensor<int>? BuildBatterySensor(string propertyName, string alias, string propertyValue, bool withMultiplier = false )
        {
            return int.TryParse(propertyValue, out var value)
                ? new Sensor<int>(propertyName, alias, withMultiplier ? value * 20 : value, "%", SensorType.Battery, sensorCategory: SensorCategory.Diagnostic)
                : null;
        }

        private static Sensor<int>? BuildPPMSensor(string propertyName, string alias, string propertyValue, SensorType sensorType, bool isTotal = false)
        {
            return int.TryParse(propertyValue, out var value)
                ? new Sensor<int>(propertyName, alias, value, "ppm", sensorType, isTotal ? SensorState.Total : SensorState.Measurement)
                : null;
        }

        private static Sensor<int>? BuildParticleSensor(string propertyName, string alias, string propertyValue, SensorType sensorType, bool isMetric = true, bool isTotal = false)
        {
            return int.TryParse(propertyValue, out var value)
                ? new Sensor<int>(propertyName, alias, value, "µg/m³", sensorType, isTotal ? SensorState.Total : SensorState.Measurement)
                : null;
        }

        private static Sensor<double>? BuildVoltageSensor(string propertyName, string alias, string propertyValue, bool isDiag = false)
        {
            return double.TryParse(propertyValue, out var value)
                ? new Sensor<double>(propertyName, alias, value, "V", SensorType.Voltage, sensorCategory: isDiag ? SensorCategory.Diagnostic : SensorCategory.Config)
                : null;
        }

        private static Sensor<double>? BuildRainRateSensor(string propertyName, string alias, string propertyValue, bool isMetric)
        {
            return double.TryParse(propertyValue, out var value)
                ? new Sensor<double>(propertyName, alias, isMetric ? I2M(value) : value, isMetric ? "mm/h" : "in/h", SensorType.PrecipitationIntensity)
                : null;
        }

        private static Sensor<double>? BuildRainSensor(string propertyName, string alias, string propertyValue, bool isMetric)
        {
            return double.TryParse(propertyValue, out var value)
                ? new Sensor<double>(propertyName, alias, isMetric ? I2M(value) : value, isMetric ? "mm" : "in", SensorType.Precipitation)
                : null;
        }

        private static Sensor<double>? BuildWindSpeedSensor(string propertyName, string alias, string propertyValue, bool isMetric)
        {
            return double.TryParse(propertyValue, out var value)
                ? new Sensor<double>(propertyName, alias, isMetric ? M2K(value) : value, isMetric ? "km/h" : "mph", SensorType.WindSpeed)
                : null;
        }

        private static Sensor<double>? BuildPressureSensor(string propertyName, string alias, string propertyValue, bool isMetric)
        {
            return double.TryParse(propertyValue, out var value)
                ? new Sensor<double>(propertyName, alias, isMetric ? IM2HP(value) : value, isMetric ? "hPa" : "inHg", SensorType.Pressure)
                : null;
        }

        private static Sensor<double>? BuildHumiditySensor(string propertyName, string alias, string propertyValue)
        {
            return double.TryParse(propertyValue, out var value)
                ? new Sensor<double>(propertyName, alias, value, "%", SensorType.Humidity)
                : null;
        }

        private static Sensor<double>? BuildTemperatureSensor(string propertyName, string alias, string propertyValue, bool isMetric, bool startMetric = false)
        {
            if (double.TryParse(propertyValue, out var value))
            {
                var unit = isMetric ? "°C" : "F";
                if (startMetric != isMetric)
                {
                    value = startMetric ? C2F(value) : F2C(value);
                }

                return new Sensor<double>(propertyName, alias, value, unit, SensorType.Temperature);
            }

            return null;
        }

        private static Sensor<double>? BuildDoubleSensor(string propertyName, string alias, string propertyValue, string unit = "", SensorType type = SensorType.None, bool isDiag = false)
        {
            return double.TryParse(propertyValue, out var value)
                ? new Sensor<double>(propertyName, alias,value, unit, type, sensorCategory: isDiag ? SensorCategory.Diagnostic : SensorCategory.Config) 
                : null;
        }

        private static Sensor<DateTime>? BuildDateTimeSensor(string propertyName, string alias, string propertyValue)
        {
            return long.TryParse(propertyValue, out var ts)
                ? new Sensor<DateTime>(propertyName, alias, DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime)
                : null;
        }

        private static Sensor<int>? BuildIntSensor(string propertyName, string alias, string propertyValue, string unit = "", SensorType type = SensorType.None, bool isDiag = false)
        {
            return int.TryParse(propertyValue, out var value)
                ? new Sensor<int>(propertyName, alias, value, unit, type, sensorCategory: isDiag ? SensorCategory.Diagnostic : SensorCategory.Config)
                : null;
        }

        // well, that (https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-source-generators?pivots=dotnet-8-0) doesn't work
        //[GeneratedRegex(@"(\D*)(\d+)", RegexOptions.IgnoreCase)]
        //private static partial Regex SensorNumberRegex();
        // so we're doing it old school
        private static int GetNumber(string propertyName)
        {
            const string pattern = @"^[a-zA-Z_0-9]*(\d+)$";
            var m = Regex.Match(propertyName, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return m.Success ? int.Parse(m.Groups[1].Value) : -1;
        }

        private static double K2M(double result)
        {
            return result * 0.621371;
        }

        private static double IM2HP(double im)
        {
            return im * 0.0338639;
        }

        private static double F2C(double fahrenheit)
        {
            return ((fahrenheit - 32) * 5 / 9);
        }

        private static double C2F(double celsius)
        {
            return celsius * 9 / 5 + 32;
        }

        private static double M2K(double mph)
        {
            return mph * 1.60934;
        }

        private static double I2M(double inches)
        {
            return inches * 25.4;
        }

        private static double L2G(double liters)
        {
            return liters * 0.264172;
        }
    }
}
