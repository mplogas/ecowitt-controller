namespace Ecowitt.Controller.Configuration
{
    public class EcowittOptions
    {
        public List<Gateway> Gateways { get; set; } = new List<Gateway>();
    }
    
    public class Gateway
    {
        public string Name { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; } = 80;
        public int Retries { get; set; } = 2;
    }
}
