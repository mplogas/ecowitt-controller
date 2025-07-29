namespace Ecowitt.Controller.Model.Message.Event;

public class HttpServiceEvent
{
    public HttpServiceEventType EventType { get; set; } = HttpServiceEventType.Unknown;
    public string? Message { get; set; }

}

public enum HttpServiceEventType
{
    Unknown,
    Started,
    Stopped,
    Error
}