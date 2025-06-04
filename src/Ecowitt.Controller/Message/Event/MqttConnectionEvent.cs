namespace Ecowitt.Controller.Message.Event;

public class MqttConnectionEvent
{
    public EventType EventType { get; set; }
    public string? Message { get; set; }
}

public enum EventType
{
    Connected,
    Disconnected,
    Error,
    MessageReceived
}