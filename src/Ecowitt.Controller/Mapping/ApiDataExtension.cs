using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
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
            // @deGir says, I shoud drop Newtonsoft for System.Text.Json because of ~~reasons~~
            var jsonDocument = JsonDocument.Parse(subdeviceApiData.Payload);
            var commands = jsonDocument.RootElement.GetProperty("command");


            foreach (var element in commands.EnumerateArray())
            {
                foreach (var property in element.EnumerateObject())
                {
                    var propertyName = property.Name;
                    var propertyValue = property.Value.ToString();
                    if (propertyName.Equals("devicename", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.Devicename = propertyValue;
                        continue;
                    }
                    
                    if (propertyName.Equals("nickname", StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.Nickname = propertyValue;
                        continue;
                    }
                    
                    var sensor = SensorBuilder.BuildSensor(propertyName, propertyValue, isMetric);
                    if (sensor != null)
                    {
                        result.Sensors.Add(sensor);
                    }
                    
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