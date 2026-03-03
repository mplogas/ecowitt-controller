using Ecowitt.Controller.Model.Configuration;

namespace Ecowitt.Controller.Model.Message.Config;

public class MqttConfig
{
    public bool Enabled { get; set; } = true;
    public string Host { get; set; }
    public int Port { get; set; } = 1883;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BaseTopic { get; set; } = "ecowitt";
    public string ClientId { get; set; } = "ecowitt-controller";
    public bool Reconnect { get; set; } = true;
    public int ReconnectAttempts { get; set; } = 2;
    public int Precision { get; set; } = 2;
    public Units Units { get; set; } = Units.Metric;
    public bool HomeAssistantDiscovery { get; set; } = true;
    
    public string CmdTopic { get; } = "cmd";
    public string HeartbeatTopic { get; } = "heartbeat";
}