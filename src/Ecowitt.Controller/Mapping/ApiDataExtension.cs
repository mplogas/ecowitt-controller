using System.Reflection;
using System.Xml.Linq;
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
                            (int?)device.run_time, "", SensorType.None, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("rssi", 
                            (int?)device.rssi, "", SensorType.SignalStrength, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("gw_rssi", 
                            (int?)device.gw_rssi, "", SensorType.SignalStrength, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("ac_action",
                            (int?)device.ac_action, "", SensorType.None, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic));
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
                            (int?)device.run_time, "", SensorType.None, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("rssi", 
                            (int?)device.rssi, "", SensorType.SignalStrength, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("gw_rssi", 
                            (int?)device.gw_rssi, "", SensorType.SignalStrength, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("water_action",
                            (int?)device.water_action, "", SensorType.None, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
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
                            (int?)device.run_time, "", SensorType.None, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("rssi", 
                            (int?)device.rssi, "", SensorType.SignalStrength, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("gw_rssi", 
                            (int?)device.gw_rssi, "", SensorType.SignalStrength, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("plan_status",
                            (int?)device.plan_status, "", SensorType.None, SensorState.Measurement, sensorCategory: SensorCategory.Diagnostic));
                        break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Maps the GatewayApiData JSON to a Gateway object, including all sensors. Applies metric/freedom units conversion if required.
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
            //DateUtc = gatewayApiData.DateUtc,
            IpAddress = gatewayApiData.IpAddress ?? string.Empty,
            TimestampUtc = gatewayApiData.TimestampUtc
        };

        if (!string.IsNullOrWhiteSpace(gatewayApiData.Payload) && gatewayApiData.Payload != "200 OK")
        {
            dynamic? json = JsonConvert.DeserializeObject(gatewayApiData.Payload);
            if (json != null)
            {
                foreach (var item in json)
                {
                    if (item?.name == null || item?.value == null) continue;
                    string propertyName = item.name.ToString();
                    string propertyValue = item.value.ToString(); //this is always a string, thanks ecowitt!

                    //filtering out keys we already set
                    if (propertyName.Equals("PASSKEY", StringComparison.InvariantCultureIgnoreCase) ||
                        propertyName.Equals("model", StringComparison.InvariantCultureIgnoreCase)
                        || propertyName.Equals("stationtype", StringComparison.InvariantCultureIgnoreCase) ||
                        propertyName.Equals("runtime", StringComparison.InvariantCultureIgnoreCase)
                        || propertyName.Equals("freq", StringComparison.InvariantCultureIgnoreCase) ||
                        propertyName.Equals("dateutc", StringComparison.InvariantCultureIgnoreCase)
                        || propertyName.Equals("model", StringComparison.InvariantCultureIgnoreCase)) continue;

                    ISensor sensor = null;

                    if (propertyName.StartsWith("Temp", StringComparison.InvariantCultureIgnoreCase) &&
                        propertyName.EndsWith("f", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (double.TryParse(propertyValue, out double value))
                        {
                            sensor = new Sensor<double?>(propertyName, isMetric ? propertyName.Remove(propertyName.Length - 1) : propertyName, isMetric ? F2C(value/100) : value/100, isMetric ? "°C" : "F",
                                SensorType.Temperature, SensorState.Measurement);
                        }
                    }
                    else if (propertyName.Equals("tf_co2", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (double.TryParse(propertyValue, out double value))
                        {
                            sensor = new Sensor<double?>(propertyName, isMetric ? "temp_co2" : "tempf_co2", isMetric ? F2C(value / 100) : value / 100, isMetric ? "°C" : "F",
                                SensorType.Temperature, SensorState.Measurement);
                        }
                    }
                    else if (propertyName.StartsWith("Humi", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (int.TryParse(propertyValue, out int value))
                        {
                            sensor = new Sensor<int?>(propertyName, value, "%", SensorType.Humidity,
                                SensorState.Measurement);
                        }
                    }
                    else if (propertyName.StartsWith("Barom", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (double.TryParse(propertyValue, out double value))
                        {
                            sensor = new Sensor<double?>(propertyName, isMetric ? IM2HP(value) : value,
                                isMetric ? "hPa" : "inHg", SensorType.Pressure, SensorState.Measurement);
                        }
                    }
                    else if (propertyName.StartsWith("WindDir", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (int.TryParse(propertyValue, out int value))
                        {
                            sensor = new Sensor<int?>(propertyName, value, "°", SensorType.None, SensorState.Measurement);
                        }
                    }
                    else if ((propertyName.StartsWith("Wind", StringComparison.InvariantCultureIgnoreCase) &&
                              propertyName.EndsWith("Mph", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (double.TryParse(propertyValue, out double value))
                        {
                            sensor = new Sensor<double?>(propertyName, isMetric ? propertyName.Remove(propertyName.Length-3) : propertyName, isMetric ? M2K(value/100) : value/100,
                                isMetric ? "km/h" : "mph", SensorType.WindSpeed, SensorState.Measurement);
                        }
                    }
                    else if ((propertyName.Equals("MaxDailyGust", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (double.TryParse(propertyValue, out double value))
                        {
                            sensor = new Sensor<double?>(propertyName, isMetric ? M2K(value/100) : value / 100,
                                isMetric ? "km/h" : "mph", SensorType.WindSpeed, SensorState.Measurement);
                        }
                    }
                    else if (propertyName.StartsWith("SolarRadiation", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (double.TryParse(propertyValue, out double value))
                        {
                            sensor = new Sensor<double?>(propertyName, value/100, "W/m²", SensorType.Illuminance,
                                SensorState.Measurement);
                        }
                    }
                    else if (propertyName.StartsWith("Uv", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (int.TryParse(propertyValue, out int value))
                        {
                            sensor = new Sensor<int?>(propertyName, value, "UV", SensorType.None, SensorState.Measurement);
                        }
                    }
                    else if (propertyName.Contains("rain", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (double.TryParse(propertyValue, out double value))
                        {
                            var stateClass = SensorState.Measurement;
                            if (propertyName.StartsWith("TotalRain", StringComparison.InvariantCultureIgnoreCase)) stateClass = SensorState.TotalIncreasing;
                            else if (propertyName.StartsWith("HourlyRain", StringComparison.InvariantCultureIgnoreCase) ||
                                     propertyName.StartsWith("DailyRain", StringComparison.InvariantCultureIgnoreCase) ||
                                     propertyName.StartsWith("WeeklyRain", StringComparison.InvariantCultureIgnoreCase) ||
                                     propertyName.StartsWith("MonthlyRain", StringComparison.InvariantCultureIgnoreCase) ||
                                     propertyName.StartsWith("YearlyRain", StringComparison.InvariantCultureIgnoreCase) ||
                                     propertyName.StartsWith("hrain", StringComparison.InvariantCultureIgnoreCase) ||
                                     propertyName.StartsWith("drain", StringComparison.InvariantCultureIgnoreCase) ||
                                     propertyName.StartsWith("wrain", StringComparison.InvariantCultureIgnoreCase) ||
                                     propertyName.StartsWith("mrain", StringComparison.InvariantCultureIgnoreCase) ||
                                     propertyName.StartsWith("yrain", StringComparison.InvariantCultureIgnoreCase))
                                stateClass = SensorState.Total;

                            sensor = new Sensor<double?>(propertyName, isMetric ? I2M(value)/1000 : value/1000, isMetric ? "mm" : "in", SensorType.Precipitation, stateClass);
                        }
                    } else if (propertyName.StartsWith("SoilMoisture", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (int.TryParse(propertyValue, out int value))
                        {
                            sensor = new Sensor<int?>(propertyName, value, "%", SensorType.Moisture, SensorState.Measurement);
                        }
                    } else if (propertyName.StartsWith("SoilAd", StringComparison.InvariantCultureIgnoreCase)) {
                        if (int.TryParse(propertyValue, out int value))
                        {
                            sensor = new Sensor<int?>(propertyName, value, "", SensorType.None, SensorState.Measurement);
                        }
                    } else if (propertyName.StartsWith("Pm25", StringComparison.InvariantCultureIgnoreCase)) {
                        var stateClass = propertyName.Contains("_24h", StringComparison.InvariantCultureIgnoreCase) ? SensorState.Total : SensorState.Measurement;

                        if (double.TryParse(propertyValue, out double value))
                        {
                            sensor = new Sensor<double?>(propertyName, value/10, "µg/m³", SensorType.Pm25, stateClass);
                        }
                    } else if (propertyName.StartsWith("Pm10", StringComparison.InvariantCultureIgnoreCase)) {
                        var stateClass = propertyName.Contains("_24h", StringComparison.InvariantCultureIgnoreCase) ? SensorState.Total : SensorState.Measurement;

                        if (double.TryParse(propertyValue, out double value))
                        {
                            sensor = new Sensor<double?>(propertyName, value/10, "µg/m³", SensorType.Pm10, stateClass);
                        }
                    } else if (propertyName.StartsWith("Pm1", StringComparison.InvariantCultureIgnoreCase)) {
                        var stateClass = propertyName.Contains("_24h", StringComparison.InvariantCultureIgnoreCase) ? SensorState.Total : SensorState.Measurement;

                        if (double.TryParse(propertyValue, out double value))
                        {
                            sensor = new Sensor<double?>(propertyName, value / 10, "µg/m³", SensorType.Pm1, stateClass);
                        }
                    } else if (propertyName.StartsWith("Co2", StringComparison.InvariantCultureIgnoreCase) && !propertyName.Equals("co2_batt", StringComparison.InvariantCultureIgnoreCase)) {
                        var stateClass = propertyName.Contains("_24h", StringComparison.InvariantCultureIgnoreCase) ? SensorState.Total : SensorState.Measurement;

                        if (int.TryParse(propertyValue, out int value))
                        {
                            sensor = new Sensor<int?>(propertyName, value, "ppm", SensorType.CarbonDioxide, stateClass);
                        }
                    } else if (propertyName.StartsWith("Lightning_time", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var value = T2TS(propertyValue);
                        if (value != null)
                        {
                            sensor = new Sensor<DateTime?>(propertyName, value, "", SensorType.Timestamp, SensorState.Measurement);
                        }
                        else
                        {
                            sensor = new Sensor<string?>(propertyName, propertyValue, "", SensorType.Timestamp, SensorState.Measurement);
                        }
                    } else if (propertyName.StartsWith("Lightning_num", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (int.TryParse(propertyValue, out int value))
                        {
                            sensor = new Sensor<int?>(propertyName, value, "", SensorType.None, SensorState.Measurement);
                        }
                    } else if (propertyName.StartsWith("Lightning", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (double.TryParse(propertyValue, out double value))
                        {
                            sensor = new Sensor<double?>(propertyName, "ligthning_dist", isMetric ? value : K2M(value), isMetric ? "km" : "miles", SensorType.Distance, SensorState.Measurement);
                        } /*else {
                            var sensor = new Sensor<string?>(propertyName, propertyValue, "km", SensorType.Distance, SensorState.Measurement);
                            result.Sensors.Add(sensor);
                        }*/
                    } else if (propertyName.StartsWith("Leak", StringComparison.InvariantCultureIgnoreCase)) {
                        if (int.TryParse(propertyValue, out int value))
                        {
                            sensor = new Sensor<int?>(propertyName, value, "", SensorType.None, SensorState.Measurement);
                        }
                    } else if (propertyName.StartsWith("Tf", StringComparison.InvariantCultureIgnoreCase)) {
                        if (double.TryParse(propertyValue, out double value))
                        {
                            sensor = new Sensor<double?>(propertyName, value, "", SensorType.None, SensorState.Measurement);
                        }
                    } else if (propertyName.StartsWith("Leaf_Wetness", StringComparison.InvariantCultureIgnoreCase)) {
                        if (int.TryParse(propertyValue, out int value))
                        {
                            sensor = new Sensor<int?>(propertyName, value, "%", SensorType.Humidity, SensorState.Measurement);
                        }
                    } else if (propertyName.Contains("Batt", StringComparison.InvariantCultureIgnoreCase) || propertyName.EndsWith("volt", StringComparison.InvariantCultureIgnoreCase)) {
                        if (int.TryParse(propertyValue, out int intVal))
                        {
                            if (propertyName.Equals("co2_batt", StringComparison.InvariantCultureIgnoreCase))
                            {
                                sensor = intVal == 6 ? new Sensor<int?>(propertyName, 100, "%", SensorType.Battery, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic) : new Sensor<int?>(propertyName, intVal * 20, "%", SensorType.Battery, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic);
                            } else if (propertyName.Equals("wh57batt", StringComparison.InvariantCultureIgnoreCase))
                            {
                                sensor = new Sensor<int?>(propertyName, "lightning_batt", intVal * 20, "%", SensorType.Battery, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic);
                            }
                            else
                            {
                                sensor = new Sensor<int?>(propertyName, intVal, "%", SensorType.Battery, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic);
                            }
                        } else if (double.TryParse(propertyValue, out double doubleVal))
                        {
                            double? value;
                            if (propertyName.Equals("wh90batt", StringComparison.InvariantCultureIgnoreCase))
                            {
                                value = doubleVal / 100;
                            }
                            else
                            {
                                value = doubleVal / 10;
                            }
                            sensor = new Sensor<double?>(propertyName, value, "V", SensorType.Battery, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic);
                        }
                        else
                        {
                            sensor = new Sensor<string?>(propertyName, propertyValue.ToString(), "", SensorType.Battery, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic);
                        }
                    } else if (propertyName.Equals("heap", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (int.TryParse(propertyValue, out int value))
                        {
                            sensor = new Sensor<int?>(propertyName, value, "byte", SensorType.None, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic);
                        }
                    }  else if (propertyName.Equals("interval", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (int.TryParse(propertyValue, out int value))
                        {
                            sensor = new Sensor<int?>(propertyName, value, "", SensorType.None, SensorState.Measurement, SensorClass.Sensor, SensorCategory.Diagnostic);
                        }
                    } else {
                        Log.Warning($"Unknown property {propertyName} found in GatewayApiData.");
                    }

                    if(sensor != null)
                    {
                        result.Sensors.Add(sensor);
                    }
                }
            }
        }
        return result;
    }

    private static DateTime? T2TS(string value)
    {
        if(long.TryParse(value, out var ts))
        {
            return DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime;
        }

        return null;
    }

    private static double? K2M(double result)
    {
        return result * 0.621371;
    }

    private static double? IM2HP(double im)
    {
        return im * 0.0338639;
    }

    private static double? F2C(double? fahrenheit)
    {
        return ((fahrenheit - 32) * 5 / 9);
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