using Ecowitt.Controller.Model;
using System.Text.Json;

namespace Ecowitt.Controller.Mapping;

public static class ApiDataExtension
{
    /// <summary>
    /// Maps the SubdeviceApiData to a Subdevice object, including all sensors. Applies metric/freedom units conversion if required.
    /// </summary>
    /// <param name="subdeviceApiData"></param>
    /// <param name="isMetric"></param>
    /// <returns></returns>
    public static Model.Subdevice Map(this SubdeviceApiData subdeviceApiData, bool isMetric = true, bool calculateValues = true)
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

            if (calculateValues)
            {
                if (result.Model == SubdeviceModel.WFC01)
                {
                    SensorBuilder.CalculateWFC01Addons(ref result);
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
    public static Gateway Map(this GatewayApiData gatewayApiData, bool isMetric = true, bool calculateValues = true)
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
            using var jsonDocument = JsonDocument.Parse(gatewayApiData.Payload);
            foreach (var element in jsonDocument.RootElement.EnumerateArray())
            {
                var propertyName = element.GetProperty("name").GetString();
                var propertyValue = element.GetProperty("value").GetString();

                if(string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(propertyValue)) continue;

                var sensor = SensorBuilder.BuildSensor(propertyName, propertyValue, isMetric);
                if (sensor != null)
                {
                    result.Sensors.Add(sensor);
                }
            }

            if (calculateValues)
            {
                SensorBuilder.CalculateGatewayAddons(ref result, isMetric);
            }
        }
        return result;
    }
}