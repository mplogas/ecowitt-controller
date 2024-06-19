namespace Ecowitt.Controller.Model;

public class SubdeviceCommand
{
    public string Cmd { get; set; }
    public int Id { get; set; }
    public int Model { get; set; }
    public string Payload { get; set; } //inputobj json
}