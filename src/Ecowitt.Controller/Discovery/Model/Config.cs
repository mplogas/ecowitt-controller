namespace Ecowitt.Controller.Discovery.Model;

public class Config
{
    public Device Device { get; set; }
    public Origin Origin { get; set; }  
    public string Name { get; set; }
    public bool? Retain { get; set; }
    public int? Qos { get; set; }
    public string Availability_Topic { get; set; }
    public string State_Topic { get; set; }
    public string? Command_Topic { get; set; }
    public string Unique_Id { get; set; }
    public string Object_Id { get; set; }
    public string? Unit_Of_Measurement { get; set; }
    public string? Icon { get; set; }
    public List<Availability>? Availability { get; set; }
    public AvailabilityMode? Availability_Mode { get; set; }
    public string Value_Template { get; set; }
}   