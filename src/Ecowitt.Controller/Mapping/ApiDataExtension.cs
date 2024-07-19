using Ecowitt.Controller.Model;
using Newtonsoft.Json;

namespace Ecowitt.Controller.Mapping;

public static class ApiDataExtension
{
    /// <summary>
    /// Maps the SubdeviceApiData to a Subdevice object, including all sensors. Applies metric/freedom units conversion if required.
    /// </summary>
    /// <param name="subdeviceApiData"></param>
    /// <param name="isMetric"></param>
    /// <returns></returns>
    public static Model.Subdevice Map(this SubdeviceApiData subdeviceApiData, bool isMetric = true)
    {
        var result = new Model.Subdevice
        {
            Id = subdeviceApiData.Id,
            Model = (SubdeviceModel)subdeviceApiData.Model,
            Availability = subdeviceApiData.RfnetState == 1,
            GwIp = subdeviceApiData.GwIp,
            TimestampUtc = subdeviceApiData.TimestampUtc,
            Version = subdeviceApiData.Version
        };


        if (!string.IsNullOrWhiteSpace(subdeviceApiData.Payload))
        {
            dynamic? json = JsonConvert.DeserializeObject(subdeviceApiData.Payload);
            if (json != null)
            {
                var device = json.command[0];
                result.Devicename = device.devicename;
                result.Nickname = device.nickname;
                switch (result.Model)
                {
                    case SubdeviceModel.AC1100:
                        result.Sensors.Add(new Sensor<bool>("AC Status", SensorType.None, SensorState.Measurement, "",
                            device.ac_status == 1, SensorClass.BinarySensor));
                        result.Sensors.Add(new Sensor<bool>("AC Running", SensorType.None, SensorState.Measurement, "",
                            device.ac_running == 1, SensorClass.BinarySensor));
                        result.Sensors.Add(new Sensor<bool>("Warning", SensorType.None, SensorState.Measurement, "",
                            device.warning == 1, SensorClass.BinarySensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<bool>("Always On", SensorType.None, SensorState.Measurement, "",
                            device.always_on == 1, SensorClass.BinarySensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("Val_Type", SensorType.None, SensorState.Measurement, "",
                            device.val_type, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("Val", SensorType.None, SensorState.Measurement, "",
                            device.val, SensorClass.Sensor));
                        result.Sensors.Add(new Sensor<int>("runtime", SensorType.Timestamp, SensorState.Measurement, "",
                            device.run_time, SensorClass.Sensor));
                        result.Sensors.Add(new Sensor<int>("rssi", SensorType.SignalStrength, SensorState.Measurement,
                            "", device.rssi, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("gw_rssi", SensorType.SignalStrength,
                            SensorState.Measurement, "", device.gw_rssi, SensorClass.Sensor,
                            SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("timeutc", SensorType.Timestamp, SensorState.Measurement, "",
                            device.timeutc, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("publish_time", SensorType.Timestamp,
                            SensorState.Measurement, "", device.publish_time, SensorClass.Sensor,
                            SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("ac_action", SensorType.Enum, SensorState.Measurement, "",
                            device.ac_action, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("plan_status", SensorType.None, SensorState.Measurement, "",
                            device.plan_status, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("Total Consumption", SensorType.Power,
                            SensorState.TotalIncreasing, "Wh", device.elect_total, SensorClass.Sensor));
                        result.Sensors.Add(new Sensor<int>("Daily Consumption", SensorType.Power, SensorState.Total,
                            "Wh", device.happen_elect, SensorClass.Sensor));
                        result.Sensors.Add(new Sensor<int>("Realtime Consumption", SensorType.Power,
                            SensorState.Measurement, "W", device.realtime_power, SensorClass.Sensor));
                        result.Sensors.Add(new Sensor<int>("AC Voltage", SensorType.Voltage, SensorState.Measurement,
                            "V", device.ac_voltage, SensorClass.Sensor));
                        result.Sensors.Add(new Sensor<int>("AC Current", SensorType.Current, SensorState.Measurement,
                            "A", device.ac_current, SensorClass.Sensor));
                        break;
                    case SubdeviceModel.WFC01:
                        result.Sensors.Add(new Sensor<bool>("Water Status", SensorType.None, SensorState.Measurement,
                            "", device.water_status == 1, SensorClass.BinarySensor));
                        result.Sensors.Add(new Sensor<bool>("Water running Running", SensorType.None,
                            SensorState.Measurement, "", device.water_running == 1, SensorClass.BinarySensor));
                        result.Sensors.Add(new Sensor<bool>("Warning", SensorType.None, SensorState.Measurement, "",
                            device.warning == 1, SensorClass.BinarySensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<bool>("Always On", SensorType.None, SensorState.Measurement, "",
                            device.always_on == 1, SensorClass.BinarySensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("Val_Type", SensorType.None, SensorState.Measurement, "",
                            device.val_type, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("Val", SensorType.None, SensorState.Measurement, "",
                            device.val));
                        result.Sensors.Add(new Sensor<int>("runtime", SensorType.Timestamp, SensorState.Measurement, "",
                            device.run_time));
                        result.Sensors.Add(new Sensor<int>("rssi", SensorType.SignalStrength, SensorState.Measurement,
                            "", device.rssi, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("gw_rssi", SensorType.SignalStrength,
                            SensorState.Measurement, "", device.gw_rssi, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("timeutc", SensorType.Timestamp, SensorState.Measurement, "",
                            device.timeutc, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("publish_time", SensorType.Timestamp,
                            SensorState.Measurement, "", device.publish_time,
                            sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("water_action", SensorType.Enum, SensorState.Measurement, "",
                            device.water_action, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("plan_status", SensorType.None, SensorState.Measurement, "",
                            device.plan_status, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<double>("Total Consumption", SensorType.Volume,
                            SensorState.TotalIncreasing, isMetric ? "L" : "gal",
                            isMetric ? device.water_total : L2G(device.water_total)));
                        result.Sensors.Add(new Sensor<double>("Current Consumption", SensorType.Volume,
                            SensorState.Measurement, isMetric ? "L" : "gal",
                            isMetric ? device.happen_water : L2G(device.happen_water)));
                        result.Sensors.Add(new Sensor<double>("Flow Velocity", SensorType.VolumeFlowRate,
                            SensorState.Measurement, isMetric ? "L/min" : "gal/min",
                            isMetric ? device.flow_velocity : L2G(device.flow_velocity)));
                        result.Sensors.Add(new Sensor<double>("Water Temperature", SensorType.Temperature,
                            SensorState.Measurement, isMetric ? "°C" : "F",
                            isMetric ? device.water_temp : C2F(device.water_temp)));
                        result.Sensors.Add(new Sensor<int>("Battery", SensorType.Battery, SensorState.Measurement, "%",
                            device.wfc01batt, sensorCategory: SensorCategory.Diagnostic));
                        break;
                    case SubdeviceModel.Unknown:
                    default:
                        //these things *likely* exists
                        result.Sensors.Add(new Sensor<int>("Val_Type", SensorType.None, SensorState.Measurement, "",
                            device.val_type, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("Val", SensorType.None, SensorState.Measurement, "",
                            device.val));
                        result.Sensors.Add(new Sensor<int>("runtime", SensorType.Timestamp, SensorState.Measurement, "",
                            device.run_time));
                        result.Sensors.Add(new Sensor<int>("rssi", SensorType.SignalStrength, SensorState.Measurement,
                            "", device.rssi, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("gw_rssi", SensorType.SignalStrength,
                            SensorState.Measurement, "", device.gw_rssi, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("timeutc", SensorType.Timestamp, SensorState.Measurement, "",
                            device.timeutc, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("publish_time", SensorType.Timestamp,
                            SensorState.Measurement, "", device.publish_time,
                            sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int>("plan_status", SensorType.None, SensorState.Measurement, "",
                            device.plan_status, sensorCategory: SensorCategory.Diagnostic));
                        break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Maps the GatewayApiData to a Gateway object using reflection, including all sensors. Applies metric/freedom units conversion if required.
    /// </summary>
    /// <param name="gatewayApiData"></param>
    /// <param name="isMetric"></param>
    /// <returns></returns>
    public static Gateway Map(this GatewayApiData gatewayApiData, bool isMetric = true)
    {
        var result = new Gateway
        {
            PASSKEY = gatewayApiData.PASSKEY,
            Model = gatewayApiData.Model,
            StationType = gatewayApiData.StationType,
            Runtime = gatewayApiData.Runtime,
            Freq = gatewayApiData.Freq,
            DateUtc = gatewayApiData.DateUtc,
            IpAddress = gatewayApiData.IpAddress ?? string.Empty,
            TimestampUtc = gatewayApiData.TimestampUtc
        };

        var propertyInfo = typeof(GatewayApiData).GetProperties();
        foreach (var property in propertyInfo)
            if (property.Name.StartsWith("Temp") &&
                property.Name.EndsWith("f", StringComparison.InvariantCultureIgnoreCase))
            {
                var value = (double?)property.GetValue(gatewayApiData);
                var sensor = new Sensor<double?>(property.Name, SensorType.Temperature, SensorState.Measurement,
                    isMetric ? "°C" : "F", isMetric ? F2C(value) : value);
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("Humi"))
            {
                var sensor = new Sensor<int?>(property.Name, SensorType.Humidity, SensorState.Measurement, "%",
                    (int?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("Barom"))
            {
                var value = (double?)property.GetValue(gatewayApiData);
                var sensor = new Sensor<double?>(property.Name, SensorType.Pressure, SensorState.Measurement,
                    isMetric ? "hPa" : "inHg", isMetric ? I2M(value) : value);
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("WindDir"))
            {
                var sensor = new Sensor<int?>(property.Name, SensorType.None, SensorState.Measurement, "°",
                    (int?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if ((property.Name.StartsWith("Wind") && property.Name.EndsWith("Mph")) ||
                     property.Name.StartsWith("MaxDailyGust"))
            {
                var value = (double?)property.GetValue(gatewayApiData);
                var sensor = new Sensor<double?>(property.Name, SensorType.WindSpeed, SensorState.Measurement,
                    isMetric ? "km/h" : "mph", isMetric ? M2K(value) : value);
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("SolarRadiation"))
            {
                var sensor = new Sensor<double?>(property.Name, SensorType.Illuminance, SensorState.Measurement, "W/m²",
                    (double?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("Uv"))
            {
                var sensor = new Sensor<int?>(property.Name, SensorType.None, SensorState.Measurement, "UV",
                    (int?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("RainRate") || property.Name.StartsWith("EventRain") ||
                     property.Name.StartsWith("HourlyRain") || property.Name.StartsWith("DailyRain") ||
                     property.Name.StartsWith("WeeklyRain") || property.Name.StartsWith("MonthlyRain") ||
                     property.Name.StartsWith("YearlyRain") || property.Name.StartsWith("TotalRain"))
            {
                var value = (double?)property.GetValue(gatewayApiData);
                var stateClass = SensorState.Measurement;
                if (property.Name.StartsWith("TotalRain"))
                    stateClass = SensorState.TotalIncreasing;
                else if (property.Name.StartsWith("HourlyRain") || property.Name.StartsWith("DailyRain") ||
                         property.Name.StartsWith("WeeklyRain") || property.Name.StartsWith("MonthlyRain") ||
                         property.Name.StartsWith("YearlyRain"))
                    stateClass = SensorState.Total;

                var sensor = new Sensor<double?>(property.Name, SensorType.Precipitation, stateClass,
                    isMetric ? "mm" : "in", isMetric ? I2M(value) : value);
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("SoilMoisture"))
            {
                var sensor = new Sensor<int?>(property.Name, SensorType.Moisture, SensorState.Measurement, "%",
                    (int?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("SoilAd"))
            {
                var sensor = new Sensor<int?>(property.Name, SensorType.None, SensorState.Measurement, "",
                    (int?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("Pm25"))
            {
                var stateClass = property.Name.EndsWith("Avg24h") ? SensorState.Total : SensorState.Measurement;

                var sensor = new Sensor<double?>(property.Name, SensorType.Pm25, stateClass, "µg/m³",
                    (double?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("Pm10"))
            {
                var stateClass = property.Name.EndsWith("Avg24h") ? SensorState.Total : SensorState.Measurement;
                var sensor = new Sensor<double?>(property.Name, SensorType.Pm10, stateClass, "µg/m³",
                    (double?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("Pm1"))
            {
                var stateClass = property.Name.EndsWith("Avg24h") ? SensorState.Total : SensorState.Measurement;
                var sensor = new Sensor<double?>(property.Name, SensorType.Pm1, stateClass, "µg/m³",
                    (double?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("Co2"))
            {
                var stateClass = property.Name.EndsWith("Avg24h") ? SensorState.Total : SensorState.Measurement;
                var sensor = new Sensor<int?>(property.Name, SensorType.CarbonDioxide, stateClass, "ppm",
                    (int?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("LightningNum"))
            {
                var sensor = new Sensor<int?>(property.Name, SensorType.None, SensorState.Measurement, "",
                    (int?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("Lightning"))
            {
                var sensor = new Sensor<double?>(property.Name, SensorType.Distance, SensorState.Measurement,
                    isMetric ? "km" : "miles",
                    isMetric
                        ? M2K((double?)property.GetValue(gatewayApiData))
                        : (double?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("LightningTime"))
            {
                var sensor = new Sensor<string?>(property.Name, SensorType.Timestamp, SensorState.Measurement, "",
                    (string?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("Leak"))
            {
                var sensor = new Sensor<int?>(property.Name, SensorType.None, SensorState.Measurement, "",
                    (int?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("Tf"))
            {
                var sensor = new Sensor<double?>(property.Name, SensorType.None, SensorState.Measurement, "",
                    (double?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.StartsWith("LeafWetness"))
            {
                var sensor = new Sensor<int?>(property.Name, SensorType.Humidity, SensorState.Measurement, "%",
                    (int?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }
            else if (property.Name.Contains("Batt", StringComparison.InvariantCultureIgnoreCase))
            {
                var sensor = new Sensor<double?>(property.Name, SensorType.Battery, SensorState.Measurement, "%",
                    (double?)property.GetValue(gatewayApiData));
                result.Sensors.Add(sensor);
            }

        return result;
    }

    private static double? F2C(double? fahrenheit)
    {
        return (fahrenheit - 32) * 5 / 9;
    }

    private static double? C2F(double? celsius)
    {
        return celsius * 9 / 5 + 32;
    }

    private static double? M2K(double? mph)
    {
        return mph * 1.60934;
    }

    private static double? I2M(double? inches)
    {
        return inches * 25.4;
    }

    private static double? L2G(double? liters)
    {
        return liters * 0.264172;
    }
}