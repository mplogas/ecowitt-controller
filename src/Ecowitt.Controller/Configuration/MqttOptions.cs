namespace Ecowitt.Controller.Configuration
{
    public class MqttOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string BaseTopic { get; set; }
    }
}
