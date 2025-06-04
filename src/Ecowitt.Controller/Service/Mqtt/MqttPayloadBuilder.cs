using Ecowitt.Controller.Model;

namespace Ecowitt.Controller.Service.Mqtt;

public static class MqttPayloadBuilder
{      
    public static dynamic BuildSubdevicePayload(Model.Subdevice subdevice)
    {
        return new
        {
            id = subdevice.Id,
            model = subdevice.Model,
            devicename = subdevice.Devicename,
            nickname = subdevice.Nickname,
            state = subdevice.Availability ? "online" : "offline",
            ver = subdevice.Version
        };
    }

    public static dynamic BuildSensorPayload(ISensor s, int? precision)
    {
        // _logger.LogDebug($"Sensor {s.Name} datatype: {s.DataType}"); // Logging removed, can be done by caller
        return new
        {
            name = s.Name,
            alias = s.Alias,
            value = s.DataType == typeof(double) ? Math.Round(Convert.ToDouble(s.Value), precision ?? 2) : s.Value,
            unit = !string.IsNullOrWhiteSpace(s.UnitOfMeasurement) ? s.UnitOfMeasurement : null
            //type = s.SensorType != SensorType.None ? s.SensorType.ToString() : null
        };
    }

    public static dynamic BuildGatewayPayload(Device gw, int? publishingInterval)
    {
        if (string.IsNullOrWhiteSpace(gw.Model))
        {
            return new
            {
                ip = gw.IpAddress,
                name = gw.Name
            };
        }
        
        var state = "offline"; // Default state
        if (publishingInterval.HasValue)
        {
            // Determine state based on last update time and publishing interval
            state = (DateTime.UtcNow - gw.TimestampUtc).TotalSeconds < publishingInterval.Value * 3 ? "online" : "offline";
        }
        
        return new
        {
            ip = gw.IpAddress,
            name = gw.Name,
            model = gw.Model,
            passkey = gw.PASSKEY,
            stationType = gw.StationType,
            runtime = gw.Runtime,
            state = state,
            freq = gw.Freq
        };
    }
}