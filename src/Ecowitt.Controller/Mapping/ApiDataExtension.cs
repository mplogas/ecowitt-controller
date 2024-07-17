using Ecowitt.Controller.Model;

namespace Ecowitt.Controller.Mapping;

public static class ApiDataExtension
{
    public static Gateway Map(this ApiData apiData, bool isMetric = true)
    {
        var result = new Gateway()
        {
            PASSKEY = apiData.PASSKEY,
            Model = apiData.Model,
            StationType = apiData.StationType,
            Runtime = apiData.Runtime,
            Freq = apiData.Freq,
            DateUtc = apiData.DateUtc,
            IpAddress = apiData.IpAddress ?? string.Empty,
            TimestampUtc = apiData.TimestampUtc
        };

        var propertyInfo = typeof(ApiData).GetProperties();
        foreach (var property in propertyInfo)
        {
            if (property.Name.StartsWith("Temp") &&
                property.Name.EndsWith("f", StringComparison.InvariantCultureIgnoreCase))
            {
                var value = (double?)property.GetValue(apiData);
                var sensor = new Sensor<double?>(property.Name, DeviceClass.Temperature, StateClass.Measurement, isMetric ? "°C" : "F", isMetric ? F2C(value) : value);
                result.Sensors.Add(sensor);
            } else if (property.Name.StartsWith("Humi"))
            {
                var sensor = new Sensor<int?>(property.Name, DeviceClass.Humidity, StateClass.Measurement, "%", (int?)property.GetValue(apiData));
                result.Sensors.Add(sensor);
            } else if (property.Name.StartsWith("Barom"))
            {
                var value = (double?)property.GetValue(apiData);
                var sensor = new Sensor<double?>(property.Name, DeviceClass.Pressure, StateClass.Measurement, isMetric ? "hPa" : "inHg", isMetric? I2M(value) : value);
                result.Sensors.Add(sensor);
            } else if (property.Name.StartsWith("WindDir"))
            {
                var sensor = new Sensor<int?>(property.Name, DeviceClass.None, StateClass.Measurement, "°", (int?)property.GetValue(apiData));
                result.Sensors.Add(sensor);
            } else if ((property.Name.StartsWith("Wind") && property.Name.EndsWith("Mph")) ||
                       property.Name.StartsWith("MaxDailyGust"))
            {
                var value = (double?)property.GetValue(apiData);
                var sensor = new Sensor<double?>(property.Name, DeviceClass.WindSpeed, StateClass.Measurement,
                    isMetric ? "km/h" : "mph", isMetric ? M2K(value) : value);
                result.Sensors.Add(sensor);
            } else if (property.Name.StartsWith("SolarRadiation"))
            {
                var sensor = new Sensor<double?>(property.Name, DeviceClass.Illuminance, StateClass.Measurement, "W/m²",
                    (double?)property.GetValue(apiData));
            } else if (property.Name.StartsWith("Uv"))
            {
                var sensor = new Sensor<int?>(property.Name, DeviceClass.None, StateClass.Measurement, "UV", (int?)property.GetValue(apiData));
                result.Sensors.Add(sensor);
            } else if (property.Name.StartsWith("RainRate") || property.Name.StartsWith("EventRain") ||
                       property.Name.StartsWith("HourlyRain") || property.Name.StartsWith("DailyRain") ||
                       property.Name.StartsWith("WeeklyRain") || property.Name.StartsWith("MonthlyRain") ||
                       property.Name.StartsWith("YearlyRain") || property.Name.StartsWith("TotalRain"))
            {
                var value = (double?)property.GetValue(apiData);
                var stateClass = StateClass.Measurement;
                if (property.Name.StartsWith("TotalRain"))
                {
                    stateClass = StateClass.TotalIncreasing;
                } else if (property.Name.StartsWith("HourlyRain") || property.Name.StartsWith("DailyRain") ||
                           property.Name.StartsWith("WeeklyRain") || property.Name.StartsWith("MonthlyRain") ||
                           property.Name.StartsWith("YearlyRain")) 
                {
                    stateClass = StateClass.Total;
                }
                
                var sensor = new Sensor<double?>(property.Name, DeviceClass.Precipitation, stateClass,
                    isMetric ? "mm" : "in", isMetric ? I2M(value) : value);
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("SoilMoisture"))
            {
                var sensor = new Sensor<int?>(property.Name, DeviceClass.Moisture, StateClass.Measurement, "%", (int?)property.GetValue(apiData));
                result.Sensors.Add(sensor);
            } else if (property.Name.StartsWith("SoilAd"))
            {
                var sensor = new Sensor<int?>(property.Name, DeviceClass.None, StateClass.Measurement, "", (int?)property.GetValue(apiData));
                result.Sensors.Add(sensor);
            } else if (property.Name.StartsWith("Pm25"))
            {
                var stateClass = property.Name.EndsWith("Avg24h") ? StateClass.Total : StateClass.Measurement;
                
                var sensor = new Sensor<double?>(property.Name, DeviceClass.Pm25, stateClass, "µg/m³", (double?)property.GetValue(apiData));
                result.Sensors.Add(sensor); 
            } else if (property.Name.StartsWith("Pm10"))
            {
                var stateClass = property.Name.EndsWith("Avg24h") ? StateClass.Total : StateClass.Measurement;
                var sensor = new Sensor<double?>(property.Name, DeviceClass.Pm10, stateClass, "µg/m³", (double?)property.GetValue(apiData));
                result.Sensors.Add(sensor);
            } else if (property.Name.StartsWith("Pm1"))
            {
                var stateClass = property.Name.EndsWith("Avg24h") ? StateClass.Total : StateClass.Measurement;
                var sensor = new Sensor<double?>(property.Name, DeviceClass.Pm1, stateClass, "µg/m³",
                    (double?)property.GetValue(apiData));
                result.Sensors.Add(sensor);
            } else if (property.Name.StartsWith("Co2"))
            {
                var stateClass = property.Name.EndsWith("Avg24h") ? StateClass.Total : StateClass.Measurement;
                var sensor = new Sensor<int?>(property.Name, DeviceClass.CarbonDioxide, stateClass, "ppm", (int?)property.GetValue(apiData));
                result.Sensors.Add(sensor);
            } else if (property.Name.StartsWith("LightningNum"))
            {
                var sensor = new Sensor<int?>(property.Name, DeviceClass.None, StateClass.Measurement, "", (int?)property.GetValue(apiData));
                result.Sensors.Add(sensor);
            } else if (property.Name.StartsWith("Lightning"))
            {
                var sensor = new Sensor<double?>(property.Name, DeviceClass.Distance, StateClass.Measurement,
                    isMetric ? "km" : "miles",
                    isMetric ? M2K((double?)property.GetValue(apiData)) : (double?)property.GetValue(apiData));
                result.Sensors.Add(sensor);
            } else if (property.Name.StartsWith("LightningTime"))
            {
                var sensor = new Sensor<string>(property.Name, DeviceClass.Timestamp, StateClass.Measurement, "", (string)property.GetValue(apiData));
                result.Sensors.Add(sensor);
            } else if (property.Name.StartsWith("Leak")) 
            {
                var sensor = new Sensor<int?>(property.Name, DeviceClass.None, StateClass.Measurement, "", (int?)property.GetValue(apiData));
                result.Sensors.Add(sensor);
            } else if (property.Name.StartsWith("Tf"))
            {
                var sensor = new Sensor<double?>(property.Name, DeviceClass.None, StateClass.Measurement, "",
                    (double?)property.GetValue(apiData));
            } else if (property.Name.StartsWith("LeafWetness"))
            {
                var sensor = new Sensor<int?>(property.Name, DeviceClass.Humidity, StateClass.Measurement, "%",
                    (int?)property.GetValue(apiData));
                result.Sensors.Add(sensor);
            } else if (property.Name.Contains("Batt", StringComparison.InvariantCultureIgnoreCase))
            {
                var sensor = new Sensor<double?>(property.Name, DeviceClass.Battery, StateClass.Measurement, "%",
                    (double?)property.GetValue(apiData));
                result.Sensors.Add(sensor);
            }
        }

        return result;
    }
    
    private static double? F2C(double? fahrenheit)
    {
        return (fahrenheit - 32) * 5 / 9;
    }
    
    private static double? M2K(double? mph)
    {
        return mph * 1.60934;
    }
    
    private static double? I2M(double? inches)
    {
        return inches * 25.4;
    }
}