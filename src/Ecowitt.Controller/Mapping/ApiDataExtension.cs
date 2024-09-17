using System.Reflection;
using System.Xml.Linq;
using Ecowitt.Controller.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            //var jObject = JObject.Parse(subdeviceApiData.Payload);
            //var command = jObject["command"];

            dynamic? json = JsonConvert.DeserializeObject(subdeviceApiData.Payload);
            if (json != null)
            {
                var device = json.command[0];
                result.Devicename = device.devicename;
                result.Nickname = device.nickname;
                switch (result.Model)
                {
                    case SubdeviceModel.AC1100:
                        result.Sensors.Add(new Sensor<bool>("Status", device.ac_status == 1, sensorClass: SensorClass.BinarySensor, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<bool>("Running", device.ac_running == 1, sensorClass: SensorClass.BinarySensor, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<bool>("Warning", device.warning == 1, sensorClass: SensorClass.BinarySensor, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<bool>("Always On", device.always_on == 1, sensorClass: SensorClass.BinarySensor, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("ConfigValue Type", (int?)device.val_type, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("ConfigValue", (int?)device.val, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("Runtime", (int?)device.run_time, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("RSSI", (int?)device.gw_rssi, "dBm", SensorType.SignalStrength, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("Current Action", (int?)device.ac_action, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("Plan Status", (int?)device.plan_status, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("Total Consumption", (int?)device.elect_total, "Wh", SensorType.Energy, SensorState.TotalIncreasing));
                        result.Sensors.Add(new Sensor<int?>("Daily Consumption", (int?)device.happen_elect, "Wh", SensorType.Energy, SensorState.Total));
                        result.Sensors.Add(new Sensor<int?>("Realtime Power", (int?)device.realtime_power, "W", SensorType.Power));
                        result.Sensors.Add(new Sensor<int?>("AC Voltage", (int?)device.ac_voltage, "V", SensorType.Voltage));
                        result.Sensors.Add(new Sensor<int?>("AC Current", (int?)device.ac_current, "A", SensorType.Current));
                        break;
                    case SubdeviceModel.WFC01:
                        result.Sensors.Add(new Sensor<bool>("Status", device.water_status == 1, sensorClass: SensorClass.BinarySensor, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<bool>("Running", device.water_running == 1, sensorClass: SensorClass.BinarySensor, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<bool>("Warning", device.warning == 1, sensorClass: SensorClass.BinarySensor, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<bool>("Always On", device.always_on == 1, sensorClass: SensorClass.BinarySensor, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("ConfigValue Type", (int?)device.val_type, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("ConfigValue", (int?)device.val, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("Runtime", (int?)device.run_time, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("RSSI", (int?)device.gw_rssi, "dBm", SensorType.SignalStrength, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("Current Action", (int?)device.water_action, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("Plan Status", (int?)device.plan_status, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<double?>("Total Consumption", isMetric ? (double?)device.water_total : L2G(device.water_total), isMetric ? "L" : "gal", SensorType.Volume, SensorState.TotalIncreasing));
                        result.Sensors.Add(new Sensor<double?>("Daily Consumption", isMetric ? (double?)device.water_total - (double?)device.happen_water : L2G(device.water_total) - L2G(device.happen_water), isMetric ? "L" : "gal", SensorType.Volume, SensorState.Measurement));
                        result.Sensors.Add(new Sensor<double?>("Flow Velocity", isMetric ? (double?)device.flow_velocity : L2G(device.flow_velocity), isMetric ? "L/min" : "gal/min", SensorType.VolumeFlowRate));
                        result.Sensors.Add(new Sensor<double?>("Water Temperature", isMetric ? (double?)device.water_temp : C2F(device.water_temp), isMetric ? "°C" : "F", SensorType.Temperature));
                        result.Sensors.Add(new Sensor<int?>("Battery", (int?)device.wfc01batt * 20, "%", SensorType.Battery, sensorCategory: SensorCategory.Diagnostic));
                        break;
                    case SubdeviceModel.Unknown:
                    default:
                        //these things *likely* exists
                        result.Sensors.Add(new Sensor<int?>("ConfigValue Type", (int?)device.val_type, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("ConfigValue", (int?)device.val, sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("Runtime", (int?)device.run_time, "", sensorCategory: SensorCategory.Diagnostic));
                        result.Sensors.Add(new Sensor<int?>("RSSI", (int?)device.gw_rssi, "dBm", SensorType.SignalStrength, sensorCategory: SensorCategory.Diagnostic));
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

                    var sensor = SensorBuilder.BuildSensor(propertyName, propertyValue, isMetric);
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