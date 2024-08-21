using System.Text;

namespace Ecowitt.Controller.Mqtt;

public static class Helper
{
    public static string BuildMqttGatewayTopic(string gwName)
    {
        return Sanitize(gwName);
    }

    public static string BuildMqttGatewaySensorTopic(string gwName, string sensorName, string sensorType)
    {
        return Sanitize($"{gwName}/sensors/{sensorName}/{sensorType}");
    }

    public static string BuildMqttSubdeviceTopic(string gwName, string subdeviceName)
    {
        return Sanitize($"{gwName}/subdevices/{subdeviceName}");
    }

    public static string BuildMqttSubdeviceSensorTopic(string gwName, string subdeviceName, string sensorName, string sensorType)
    {
        return Sanitize($"{gwName}/subdevices/{subdeviceName}/sensors/{sensorName}/{sensorType}");
    }

    public static string Sanitize(string input)
    {
        return input.Replace(' ', '-').ToLowerInvariant();
    }
}