using Ecowitt.Controller.Configuration;

namespace Ecowitt.Controller.Message.Config;

public class MqttConfig
{
    public bool Enabled { get; set; } = true;
    public MqttOptions Configuration { get; set; } = new();
}