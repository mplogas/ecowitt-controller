using System.Reflection;
using Ecowitt.Controller.Model;
using Newtonsoft.Json;
using Serilog;

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


        if (!string.IsNullOrWhiteSpace(subdeviceApiData.Payload) && subdeviceApiData.Payload != "200 OK")
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
                        result.Sensors.Add(new Sensor<bool>("AC Status",
                            device.ac_status == 1, "", SensorType.None, SensorState.Measurement, SensorClass.BinarySensor));
                        result.Sensors.Add(new Sensor<bool>("AC Running",
                            device.ac_running == 1, "", SensorType.None, SensorState.Measurement, SensorClass.BinarySensor));
                        result.Sensors.Add(new Sensor<bool>("Warning",
                            device.warning == 1, "", SensorType.None, SensorState.Measurement, SensorClass.BinarySensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<bool>("Always On",
                            device.always_on == 1, "", SensorType.None, SensorState.Measurement, SensorClass.BinarySensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("Val_Type",
                            (int?)device.val_type, "", SensorType.None, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("Val",
                            (int?)device.val, "", SensorType.None, SensorState.Measurement, SensorClass.Sensor));
                        result.Sensors.Add(new Sensor<int?>("runtime",
                            (int?)device.run_time, "", SensorType.Timestamp, SensorState.Measurement, SensorClass.Sensor));
                        result.Sensors.Add(new Sensor<int?>("rssi", 
                            (int?)device.rssi, "", SensorType.SignalStrength, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("gw_rssi", 
                            (int?)device.gw_rssi, "", SensorType.SignalStrength, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("timeutc",
                            (int?)device.timeutc, "", SensorType.Timestamp, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("publish_time", 
                            (int?)device.publish_time, "", SensorType.Timestamp, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("ac_action",
                            (int?)device.ac_action, "", SensorType.Enum, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("plan_status",
                            (int?)device.plan_status, "", SensorType.None, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("Total Consumption", 
                            (int?)device.elect_total, "Wh", SensorType.Power, SensorState.TotalIncreasing, SensorClass.Sensor));
                        result.Sensors.Add(new Sensor<int?>("Daily Consumption", 
                            (int?)device.happen_elect, "Wh", SensorType.Power, SensorState.Total, SensorClass.Sensor));
                        result.Sensors.Add(new Sensor<int?>("Realtime Consumption", 
                            (int?)device.realtime_power, "W", SensorType.Power, SensorState.Measurement, SensorClass.Sensor));
                        result.Sensors.Add(new Sensor<int?>("AC Voltage", 
                            (int?)device.ac_voltage, "V", SensorType.Voltage, SensorState.Measurement, SensorClass.Sensor));
                        result.Sensors.Add(new Sensor<int?>("AC Current", 
                            (int?)device.ac_current, "A", SensorType.Current, SensorState.Measurement, SensorClass.Sensor));
                        break;
                    case SubdeviceModel.WFC01:
                        result.Sensors.Add(new Sensor<bool>("Water Status", device.water_status == 1, "", SensorType.None, SensorState.Measurement, SensorClass.BinarySensor));
                        result.Sensors.Add(new Sensor<bool>("Water Running", device.water_running == 1, "", SensorType.None, SensorState.Measurement, SensorClass.BinarySensor));
                        result.Sensors.Add(new Sensor<bool>("Warning",
                            device.warning == 1, "", SensorType.None, SensorState.Measurement, SensorClass.BinarySensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<bool>("Always On",
                            device.always_on == 1, "", SensorType.None, SensorState.Measurement, SensorClass.BinarySensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("Val_Type",
                            (int?)device.val_type, "", SensorType.None, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("Val",
                            (int?)device.val, "", SensorType.None, SensorState.Measurement));
                        result.Sensors.Add(new Sensor<int?>("runtime",
                            (int?)device.run_time, "", SensorType.Timestamp, SensorState.Measurement));
                        result.Sensors.Add(new Sensor<int?>("rssi", 
                            (int?)device.rssi, "", SensorType.SignalStrength, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("gw_rssi", 
                            (int?)device.gw_rssi, "", SensorType.SignalStrength, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("timeutc",
                            (int?)device.timeutc, "", SensorType.Timestamp, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("publish_time", 
                            (int?)device.publish_time, "", SensorType.Timestamp, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("water_action",
                            (int?)device.water_action, "", SensorType.Enum, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("plan_status",
                            (int?)device.plan_status, "", SensorType.None, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<double?>("Total Consumption",
                            isMetric ? (double?)device.water_total : L2G(device.water_total), isMetric ? "L" : "gal", SensorType.Volume, SensorState.TotalIncreasing));
                        result.Sensors.Add(new Sensor<double?>("Current Consumption",
                            isMetric ? (double?)device.happen_water : L2G(device.happen_water), isMetric ? "L" : "gal", SensorType.Volume, SensorState.Measurement));
                        result.Sensors.Add(new Sensor<double?>("Flow Velocity",
                            isMetric ? (double?)device.flow_velocity : L2G(device.flow_velocity), isMetric ? "L/min" : "gal/min", SensorType.VolumeFlowRate, SensorState.Measurement));
                        result.Sensors.Add(new Sensor<double?>("Water Temperature",
                            isMetric ? (double?)device.water_temp : C2F(device.water_temp), isMetric ? "°C" : "F", SensorType.Temperature, SensorState.Measurement));
                        result.Sensors.Add(new Sensor<int?>("Battery",
                            (int?)device.wfc01batt, "%", SensorType.Battery, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        break;
                    case SubdeviceModel.Unknown:
                    default:
                        //these things *likely* exists
                        result.Sensors.Add(new Sensor<int?>("Val_Type",
                            (int?)device.val_type, "", SensorType.None, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("Val",
                            (int?)device.val, "", SensorType.None, SensorState.Measurement));
                        result.Sensors.Add(new Sensor<int?>("runtime",
                            (int?)device.run_time, "", SensorType.Timestamp, SensorState.Measurement));
                        result.Sensors.Add(new Sensor<int?>("rssi", 
                            (int?)device.rssi, "", SensorType.SignalStrength, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("gw_rssi", 
                            (int?)device.gw_rssi, "", SensorType.SignalStrength, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("timeutc",
                            (int?)device.timeutc, "", SensorType.Timestamp, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("publish_time", 
                            (int?)device.publish_time, "", SensorType.Timestamp, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("plan_status",
                            (int?)device.plan_status, "", SensorType.None, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
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

        if (!string.IsNullOrWhiteSpace(gatewayApiData.Payload) && gatewayApiData.Payload != "200 OK")
        {
            dynamic? json = JsonConvert.DeserializeObject(gatewayApiData.Payload);
            if (json != null)
            {
                Log.Debug($"gateway payload: {json}");
            }
        }

                //var propertyInfo = typeof(GatewayApiData).GetProperties();
                //foreach (var property in propertyInfo)
                //{
                //    var valueCheck = property.GetValue(gatewayApiData);
                //    if (valueCheck == null) continue;

                //    if (property.Name.StartsWith("Temp", StringComparison.InvariantCultureIgnoreCase) &&
                //        property.Name.EndsWith("f", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var value = (double?)property.GetValue(gatewayApiData);
                //        var sensor = new Sensor<double?>(property.Name, isMetric ? F2C(value) : value, isMetric ? "°C" : "F", SensorType.Temperature, SensorState.Measurement);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("Humi", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var sensor = new Sensor<int?>(property.Name,
                //            (int?)property.GetValue(gatewayApiData), "%", SensorType.Humidity, SensorState.Measurement);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("Barom", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var value = (double?)property.GetValue(gatewayApiData);
                //        var sensor = new Sensor<double?>(property.Name, isMetric ? I2M(value) : value, isMetric ? "hPa" : "inHg", SensorType.Pressure, SensorState.Measurement);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("WindDir", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var sensor = new Sensor<int?>(property.Name,
                //            (int?)property.GetValue(gatewayApiData), "°", SensorType.None, SensorState.Measurement);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if ((property.Name.StartsWith("Wind", StringComparison.InvariantCultureIgnoreCase) && property.Name.EndsWith("Mph", StringComparison.InvariantCultureIgnoreCase)) ||
                //             property.Name.StartsWith("MaxDailyGust", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var value = (double?)property.GetValue(gatewayApiData);
                //        var sensor = new Sensor<double?>(property.Name, isMetric ? M2K(value) : value, isMetric ? "km/h" : "mph", SensorType.WindSpeed, SensorState.Measurement);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("SolarRadiation", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var sensor = new Sensor<double?>(property.Name,
                //            (double?)property.GetValue(gatewayApiData), "W/m²", SensorType.Illuminance, SensorState.Measurement);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("Uv", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var sensor = new Sensor<int?>(property.Name,
                //            (int?)property.GetValue(gatewayApiData), "UV", SensorType.None, SensorState.Measurement);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.Contains("rain", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var value = (double?)property.GetValue(gatewayApiData);
                //        var stateClass = SensorState.Measurement;
                //        if (property.Name.StartsWith("TotalRain", StringComparison.InvariantCultureIgnoreCase))
                //            stateClass = SensorState.TotalIncreasing;
                //        else if (property.Name.StartsWith("HourlyRain", StringComparison.InvariantCultureIgnoreCase) || property.Name.StartsWith("DailyRain", StringComparison.InvariantCultureIgnoreCase) ||
                //                 property.Name.StartsWith("WeeklyRain", StringComparison.InvariantCultureIgnoreCase) || property.Name.StartsWith("MonthlyRain", StringComparison.InvariantCultureIgnoreCase) ||
                //                 property.Name.StartsWith("YearlyRain", StringComparison.InvariantCultureIgnoreCase))
                //            stateClass = SensorState.Total;

                //        var sensor = new Sensor<double?>(property.Name, isMetric ? I2M(value) : value, isMetric ? "mm" : "in", SensorType.Precipitation, stateClass);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("SoilMoisture", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var sensor = new Sensor<int?>(property.Name,
                //            (int?)property.GetValue(gatewayApiData), "%", SensorType.Moisture, SensorState.Measurement);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("SoilAd", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var sensor = new Sensor<int?>(property.Name,
                //            (int?)property.GetValue(gatewayApiData), "", SensorType.None, SensorState.Measurement);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("Pm25", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var stateClass = property.Name.EndsWith("Avg24h", StringComparison.InvariantCultureIgnoreCase) ? SensorState.Total : SensorState.Measurement;

                //        var sensor = new Sensor<double?>(property.Name,
                //            (double?)property.GetValue(gatewayApiData), "µg/m³", SensorType.Pm25, stateClass);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("Pm10", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var stateClass = property.Name.EndsWith("Avg24h", StringComparison.InvariantCultureIgnoreCase) ? SensorState.Total : SensorState.Measurement;
                //        var sensor = new Sensor<double?>(property.Name,
                //            (double?)property.GetValue(gatewayApiData), "µg/m³", SensorType.Pm10, stateClass);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("Pm1"))
                //    {
                //        var stateClass = property.Name.EndsWith("Avg24h", StringComparison.InvariantCultureIgnoreCase) ? SensorState.Total : SensorState.Measurement;
                //        var sensor = new Sensor<double?>(property.Name,
                //            (double?)property.GetValue(gatewayApiData), "µg/m³", SensorType.Pm1, stateClass);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("Co2", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var stateClass = property.Name.EndsWith("Avg24h", StringComparison.InvariantCultureIgnoreCase) ? SensorState.Total : SensorState.Measurement;
                //        var sensor = new Sensor<int?>(property.Name,
                //            (int?)property.GetValue(gatewayApiData), "ppm", SensorType.CarbonDioxide, stateClass);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("LightningNum", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var sensor = new Sensor<int?>(property.Name,
                //            (int?)property.GetValue(gatewayApiData), "", SensorType.None, SensorState.Measurement);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("Lightning", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        if(double.TryParse((string?)property.GetValue(gatewayApiData), out var value))
                //        {
                //            var sensor = new Sensor<double?>(property.Name, isMetric ? M2K(value) : value, isMetric ? "km" : "miles", SensorType.Distance, SensorState.Measurement);
                //            result.Sensors.Add(sensor);
                //        }
                //        else
                //        {
                //            var sensor = new Sensor<string?>(property.Name, (string?)property.GetValue(gatewayApiData), "km", SensorType.Distance, SensorState.Measurement);
                //            result.Sensors.Add(sensor);
                //        }

                //    }
                //    else if (property.Name.StartsWith("LightningTime", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var sensor = new Sensor<string?>(property.Name,
                //            (string?)property.GetValue(gatewayApiData), "", SensorType.Timestamp, SensorState.Measurement);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("Leak", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var sensor = new Sensor<int?>(property.Name,
                //            (int?)property.GetValue(gatewayApiData), "", SensorType.None, SensorState.Measurement);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("Tf", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var sensor = new Sensor<double?>(property.Name,
                //            (double?)property.GetValue(gatewayApiData), "", SensorType.None, SensorState.Measurement);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.StartsWith("LeafWetness", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        var sensor = new Sensor<int?>(property.Name,
                //            (int?)property.GetValue(gatewayApiData), "%", SensorType.Humidity, SensorState.Measurement);
                //        result.Sensors.Add(sensor);
                //    }
                //    else if (property.Name.Contains("Batt", StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        if(property.PropertyType == typeof(int?))
                //        {
                //            var sensor = new Sensor<int?>(property.Name,
                //                (int?)property.GetValue(gatewayApiData), "%", SensorType.Battery, SensorState.Measurement);
                //            result.Sensors.Add(sensor);
                //        } else if (property.PropertyType == typeof(double?))
                //        {
                //            var sensor = new Sensor<double?>(property.Name,
                //                (double?)property.GetValue(gatewayApiData), "V", SensorType.Voltage, SensorState.Measurement);
                //            result.Sensors.Add(sensor);
                //        }
                //        else
                //        {
                //            var sensor = new Sensor<string?>(property.Name,
                //                (string?)property.GetValue(gatewayApiData), "", SensorType.Battery, SensorState.Measurement);
                //            result.Sensors.Add(sensor);
                //        }
                //    }
                //    else
                //    {
                //       Log.Warning($"Unknown property {property.Name} in GatewayApiData.");
                //    }
                //}

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