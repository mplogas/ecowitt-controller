namespace Ecowitt.Controller.Service.Mqtt;

public static class MqttPathBuilder
{
    public static string BuildMqttGatewayTopic(string gwName)
    {
        return SanitizeSegment(gwName);
    }

    public static string BuildMqttGatewaySensorTopic(string gwName, string sensorName)
    {
        return $"{SanitizeSegment(gwName)}/sensors/{SanitizeSegment(sensorName)}";
    }

    public static string BuildMqttGatewayDiagnosticTopic(string gwName, string sensorName)
    {
        return $"{SanitizeSegment(gwName)}/diag/{SanitizeSegment(sensorName)}";
    }

    public static string BuildMqttSubdeviceTopic(string gwName, string subdeviceName)
    {
        return $"{SanitizeSegment(gwName)}/subdevices/{SanitizeSegment(subdeviceName)}";
    }

    public static string BuildMqttSubdeviceSensorTopic(string gwName, string subdeviceName, string sensorName)
    {
        return $"{SanitizeSegment(gwName)}/subdevices/{SanitizeSegment(subdeviceName)}/sensors/{SanitizeSegment(sensorName)}";
    }

    public static string BuildMqttSubdeviceDiagnosticTopic(string gwName, string subdeviceName, string sensorName)
    {
        return $"{SanitizeSegment(gwName)}/subdevices/{SanitizeSegment(subdeviceName)}/diag/{SanitizeSegment(sensorName)}";
    }

    public static string BuildMqttSubdeviceCommandTopic(string gwName, string subdeviceName)
    {
        return $"{SanitizeSegment(gwName)}/subdevices/{SanitizeSegment(subdeviceName)}/cmd";
    }

    public static string BuildMqttSubdeviceCommandTopic()
    {
        return "+/subdevices/+/cmd";
    }

    public static string BuildMqttSubdeviceHACommandTopic(string gwName, string subdeviceName)
    {
        return $"{SanitizeSegment(gwName)}/subdevices/{SanitizeSegment(subdeviceName)}/cmd/homeassistant";
    }

    public static string BuildMqttSubdeviceHACommandTopic()
    {
        return "+/subdevices/+/cmd/homeassistant";
    }

    public static string SanitizeSegment(string input)
    {
        return input
            .Replace(' ', '-')
            .Replace('/', '-')
            .Replace('#', '-')
            .Replace('+', '-')
            .ToLowerInvariant();
    }
}
