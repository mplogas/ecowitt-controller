namespace Ecowitt.Controller.Message.Event;

public class HomeAssistantStatusEvent
{
    public HomeAssistantStatusType Status { get; set; }
}

public enum HomeAssistantStatusType
{
    Unknown,
    Online,
    Offline
}