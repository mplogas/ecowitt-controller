using Ecowitt.Controller.Discovery.Model;

namespace Ecowitt.Controller.Discovery;

public static class DiscoveryBuilder
{
    /// <summary>
    /// Build device discovery object when no model, manufacturer, hwVersion, swVersion are provided (e.g. no autodisocvery of ecowitt subdevices)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="viaDevice"></param>
    /// <returns></returns>
    public static Device BuildDevice(string name, string? viaDevice = null)
    {
        return new Device
        {
            Identifiers = [BuildIdentifier(name)],
            Name = name,
            ViaDevice = viaDevice
        };
    }

    /// <summary>
    /// Build a regular device discovery object
    /// </summary>
    /// <param name="name"></param>
    /// <param name="model"></param>
    /// <param name="manufacturer"></param>
    /// <param name="hwVersion"></param>
    /// <param name="swVersion"></param>
    /// <param name="viaDevice"></param>
    /// <returns></returns>
    public static Device BuildDevice(string name, string model, string manufacturer, string hwVersion, string swVersion, string? viaDevice = null)
    {
        return new Device
        {
            Identifiers = [ BuildIdentifier(name)],
            Name = name,
            Model = model,
            Manufacturer = manufacturer,
            HwVersion = hwVersion,
            SwVersion = swVersion,
            ViaDevice = viaDevice
        };
    }

    /// <summary>
    /// Build default origin discovery object
    /// </summary>
    /// <returns></returns>
    public static Origin BuildOrigin()
    {
        return new Origin
        {
            Name = "Ecowitt Controller",
            Sw = "v0.0.1",
            Url = "https://github.com/mplogas/ecowitt-controller"
        };
    }

    /// <summary>
    /// Build origin discovery object
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sw"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    public static Origin BuildOrigin(string name, string sw, string url)
    {
        return new Origin
        {
            Name = name,
            Sw = sw,
            Url = url
        };
    }
    
    /// <summary>
    /// Build availability discovery object
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="valueTemplate"></param>
    /// <param name="payloadAvailable"></param>
    /// <param name="payloadNotAvailable"></param>
    /// <returns></returns>
    public static Availability BuildAvailability(string topic, string valueTemplate, string payloadAvailable = "online", string payloadNotAvailable = "offline")
    {
        return new Availability
        {
            Topic = topic,
            PayloadAvailable = payloadAvailable,
            PayloadUnavailable = payloadNotAvailable,
            ValueTemplate = valueTemplate
        };
    }

    /// <summary>
    ///  Build config discovery object for a gateway
    /// </summary>
    /// <param name="device"></param>
    /// <param name="origin"></param>
    /// <param name="name"></param>
    /// <param name="uniqueId"></param>
    /// <param name="stateTopic"></param>
    /// <param name="availabilityTopic"></param>
    /// <param name="retain"></param>
    /// <param name="qos"></param>
    /// <returns></returns>
    public static Config BuildConfig(Device device, Origin origin, string name, string uniqueId, string stateTopic, string availabilityTopic, bool? retain = false, int? qos = 1)
    {
        return new Config
        {
            Device = device,
            Origin = origin,
            Name = name,
            UniqueId = uniqueId,
            ObjectId = uniqueId,
            StateTopic = stateTopic,
            AvailabilityTopic = availabilityTopic,
            Retain = retain,
            Qos = qos
        };
    }

    /// <summary>
    /// build config discovery object for a subdevice
    /// </summary>
    /// <param name="device"></param>
    /// <param name="origin"></param>
    /// <param name="name"></param>
    /// <param name="uniqueId"></param>
    /// <param name="stateTopic"></param>
    /// <param name="availability"></param>
    /// <param name="availabilityMode"></param>
    /// <param name="retain"></param>
    /// <param name="qos"></param>
    /// <returns></returns>
    public static Config BuildConfig(Device device, Origin origin, string name, string uniqueId, string stateTopic, List<Availability> availability, AvailabilityMode availabilityMode = AvailabilityMode.All, bool? retain = false, int? qos = 1)
    {
        return new Config
        {
            Device = device,
            Origin = origin,
            Name = name,
            UniqueId = uniqueId,
            ObjectId = uniqueId,
            StateTopic = stateTopic,
            Availability = availability,
            AvailabilityMode = availabilityMode,  
            Retain = retain,
            Qos = qos
        };
    }

    /// <summary>
    /// Build config discovery object for a sensor
    /// </summary>
    /// <param name="device"></param>
    /// <param name="origin"></param>
    /// <param name="name"></param>
    /// <param name="uniqueId"></param>
    /// <param name="stateTopic"></param>
    /// <param name="availabilityTopic"></param>
    /// <param name="valueTemplate"></param>
    /// <param name="unitOfMeasurement"></param>
    /// <param name="icon"></param>
    /// <param name="retain"></param>
    /// <param name="qos"></param>
    /// <returns></returns>
    public static Config BuildConfig(Device device, Origin origin, string name, string uniqueId, string stateTopic, string availabilityTopic, string valueTemplate, string? unitOfMeasurement, string? icon, bool? retain = false, int? qos = 1)
    {
        return new Config
        {
            Device = device,
            Origin = origin,
            Name = name,
            UniqueId = uniqueId,
            ObjectId = uniqueId,
            StateTopic = stateTopic,
            AvailabilityTopic = availabilityTopic,
            ValueTemplate = valueTemplate,
            UnitOfMeasurement = unitOfMeasurement,
            Icon = icon,
            Retain = retain,
            Qos = qos
        };
    }

    /// <summary>
    /// Build config discovery object for a switch/toggle
    /// </summary>
    /// <param name="device"></param>
    /// <param name="origin"></param>
    /// <param name="name"></param>
    /// <param name="uniqueId"></param>
    /// <param name="stateTopic"></param>
    /// <param name="availabilityTopic"></param>
    /// <param name="commandTopic"></param>
    /// <param name="icon"></param>
    /// <param name="retain"></param>
    /// <param name="qos"></param>
    /// <returns></returns>
    public static Config BuildConfig(Device device, Origin origin, string name, string uniqueId, string stateTopic, string availabilityTopic, string commandTopic, string? icon, bool? retain = false, int? qos = 1)
    {
        return new Config
        {
            Device = device,
            Origin = origin,
            Name = name,
            UniqueId = uniqueId,
            ObjectId = uniqueId,
            StateTopic = stateTopic,
            AvailabilityTopic = availabilityTopic,
            CommandTopic = commandTopic,
            Icon = icon,
            Retain = retain,
            Qos = qos
        };
    }

    public static string BuildIdentifier(string name, string type = "config")
    {
        return $"ec_{name}_{type}";
    }
}