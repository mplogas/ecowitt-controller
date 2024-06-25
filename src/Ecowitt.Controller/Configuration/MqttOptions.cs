namespace Ecowitt.Controller.Configuration;

public class MqttOptions
{
    public string Host { get; set; }
    public int Port { get; set; } = 1883;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BaseTopic { get; set; } = "ecowitt";
    public string ClientId { get; set; } = "ecowitt-controller";
    public bool Reconnect { get; set; } = true;
    public int ReconnectAttempts { get; set; } = 2;
}