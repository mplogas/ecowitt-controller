namespace Ecowitt.Controller.Discovery.Model;

public class Availability
{
    public string Topic { get; set; }
    public string Payload_Available { get; set; }
    public string Payload_Not_Available { get; set; }
    public string Value_Template { get; set; }
}