using System.Text;

namespace Ecowitt.Controller.Mqtt;

public static class Helper
{
    public static string BuildMqttGatewayTopic(string gwName)
    {
        return Sanitize(gwName);
    }

    public static string BuildMqttGatewaySensorTopic(string gwName, string sensorName)
    {
        return Sanitize($"{gwName}/sensors/{sensorName}");
    }

    public static string BuildMqttGatewayDiagnosticTopic(string gwName, string sensorName)
    {
        return Sanitize($"{gwName}/diag/{sensorName}");
    }

    public static string BuildMqttSubdeviceTopic(string gwName, string subdeviceName)
    {
        return Sanitize($"{gwName}/subdevices/{subdeviceName}");
    }

    public static string BuildMqttSubdeviceSensorTopic(string gwName, string subdeviceName, string sensorName)
    {
        return Sanitize($"{gwName}/subdevices/{subdeviceName}/sensors/{sensorName}");
    }


    public static string BuildMqttSubdeviceDiagnosticTopic(string gwName, string subdeviceName, string sensorName)
    {
        return Sanitize($"{gwName}/subdevices/{subdeviceName}/diag/{sensorName}");
    }

    public static string BuildMqttSubdeviceCommandTopic(string gwName, string subdeviceName)
    {
        return Sanitize($"{gwName}/subdevices/{subdeviceName}/cmd");
    }

    public static string BuildMqttSubdeviceCommandTopic()
    {
        return Sanitize($"+/subdevices/+/cmd");
    }

    public static string BuildMqttSubdeviceHACommandTopic(string gwName, string subdeviceName)
    {
        return Sanitize($"{gwName}/subdevices/{subdeviceName}/cmd/homeassistant");
    }

    public static string BuildMqttSubdeviceHACommandTopic()
    {
        return Sanitize($"+/subdevices/+/cmd/homeassistant");
    }

    public static string Sanitize(string input)
    {
        return input.Replace(' ', '-').ToLowerInvariant();
    }
}