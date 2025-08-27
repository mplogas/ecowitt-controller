namespace Ecowitt.Controller.Model.Message.Data
{
    public class SubdeviceData
    {
        public string GatewayId { get; set; } = string.Empty;
        public string GatewayName { get; set; } = string.Empty;
        public int SubdeviceId { get; set; }
        public List<ISensor> ChangedSensors { get; set; } = new();
        public DateTime Timestamp { get; set; }

    }
}