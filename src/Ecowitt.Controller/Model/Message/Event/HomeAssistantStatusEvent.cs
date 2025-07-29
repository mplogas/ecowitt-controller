namespace Ecowitt.Controller.Model.Message.Event;

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