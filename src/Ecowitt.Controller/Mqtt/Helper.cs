using System.Text;

namespace Ecowitt.Controller.Mqtt;

public static class Helper
{
    public static string BuildMqttGatewayTopic(string gwName)
    {
        return gwName;
    }

    public static string BuildMqttGatewaySensorTopic(string gwName, string sensorName, string sensorType)
    {
        return $"{gwName}/sensors/{sensorName}/{sensorType}";
    }

    public static string BuildMqttSubdeviceTopic(string gwName, string subdeviceName)
    {
        return $"{gwName}/subdevices/{subdeviceName}";
    }

    public static string BuildMqttSubdeviceSensorTopic(string gwName, string subdeviceName, string sensorName, string sensorType)
    {
        return $"{gwName}/subdevices/{subdeviceName}/sensors/{sensorName}/{sensorType}";
    }
}