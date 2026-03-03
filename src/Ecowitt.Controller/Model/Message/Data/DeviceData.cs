namespace Ecowitt.Controller.Model.Message.Data;

public class DeviceData
{
    public string GatewayId { get; set; } = string.Empty;
    public string GatewayName { get; set; } = string.Empty;
    public List<ISensor> ChangedSensors { get; set; } = new();
    public DateTime Timestamp { get; set; }
}