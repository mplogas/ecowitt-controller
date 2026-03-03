namespace Ecowitt.Controller.Model.Message.Event;

public class MqttConnectionEvent
{
    public MqttConnectionEventType EventType { get; set; }
    public string? Message { get; set; }
}

public enum MqttConnectionEventType
{
    Connected,
    Disconnected,
    Error,
    MessageReceived
}