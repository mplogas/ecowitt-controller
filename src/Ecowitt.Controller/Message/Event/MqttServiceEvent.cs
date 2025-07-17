namespace Ecowitt.Controller.Message.Event
{
    public class MqttServiceEvent
    {
        public MqttServiceEventType EventType { get; set; } = MqttServiceEventType.Unknown;
        public string? Message { get; set; }
    }

    public enum MqttServiceEventType
    {
        Unknown,
        Started,
        Stopped,
        Heartbeat,
        Error
    }
}
