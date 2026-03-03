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
        object? valueOut = s.Value;
        if (s.DataType == SensorDataType.Double)
        {
            try { valueOut = Math.Round(Convert.ToDouble(s.Value), precision ?? 2); } catch { /* ignore */ }
        }
        return new
        {
            name = s.Name,
            alias = s.Alias,
            value = valueOut,
            unit = !string.IsNullOrWhiteSpace(s.UnitOfMeasurement) ? s.UnitOfMeasurement : null
        };
    }

    public static dynamic BuildGatewayPayload(Device gw)
    {
        if (string.IsNullOrWhiteSpace(gw.Model))
        {
            return new
            {
                ip = gw.IpAddress,
                name = gw.Name
            };
        }
        return new
        {
            ip = gw.IpAddress,
            name = gw.Name,
            model = gw.Model,
            passkey = gw.PASSKEY,
            stationType = gw.StationType,
            runtime = gw.Runtime,
            state = "online",
            freq = gw.Freq
        };
    }
}