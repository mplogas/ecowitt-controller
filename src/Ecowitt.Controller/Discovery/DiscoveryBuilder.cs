using Ecowitt.Controller.Discovery.Model;

namespace Ecowitt.Controller.Discovery;

public static class DiscoveryBuilder
{
    public static Device BuildDevice(string name, string model, string manufacturer, string hwVersion, string swVersion, string? viaDevice = null)
    {
        return new Device
        {
            Name = name,
            Model = model,
            Manufacturer = manufacturer,
            Hw_Version = hwVersion,
            Sw_Version = swVersion,
            Via_Device = viaDevice
        };
    }
    
    public static Origin BuildOrigin(string name, string sw, string url)
    {
        return new Origin
        {
            Name = name,
            Sw = sw,
            Url = url
        };
    }
    
    public static Availability BuildAvailability(string topic, string payloadAvailable, string payloadNotAvailable, string valueTemplate)
    {
        return new Availability
        {
            Topic = topic,
            Payload_Available = payloadAvailable,
            Payload_Not_Available = payloadNotAvailable,
            Value_Template = valueTemplate
        };
    }
    
    public static Config BuildConfig(Device device, Origin origin, string name, string uniqueId, string objectId, string stateTopic, string availabilityTopic, string valueTemplate, string? commandTopic = null, string? unitOfMeasurement = null, string? icon = null, List<Availability>? availability = null, AvailabilityMode? availabilityMode = null, bool? retain = null, int? qos = null)
    {
        return new Config
        {
            Device = device,
            Origin = origin,
            Name = name,
            Unique_Id = uniqueId,
            Object_Id = objectId,
            State_Topic = stateTopic,
            Availability_Topic = availabilityTopic,
            Value_Template = valueTemplate,
            Command_Topic = commandTopic,
            Unit_Of_Measurement = unitOfMeasurement,
            Icon = icon,
            Availability = availability,
            Availability_Mode = availabilityMode,
            Retain = retain,
            Qos = qos
        };
    }
}