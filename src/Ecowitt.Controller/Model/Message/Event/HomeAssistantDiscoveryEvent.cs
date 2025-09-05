namespace Ecowitt.Controller.Model.Message.Event;

public class HomeAssistantDiscoveryEvent(Model.Device device)
{
    public Device Device { get; set; } = device;
}