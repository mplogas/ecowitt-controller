using Ecowitt.Controller.Discovery.Model;
using Ecowitt.Controller.Model;

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
    public static Config BuildGatewayConfig(Device device, Origin origin, string name, string uniqueId, string stateTopic, string availabilityTopic, bool? retain = false, int? qos = 1)
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
    public static Config BuildSubdeviceConfig(Device device, Origin origin, string name, string uniqueId, string stateTopic, List<Availability> availability, AvailabilityMode availabilityMode = AvailabilityMode.All, bool? retain = false, int? qos = 1)
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
    public static Config BuildSensorConfig(Device device, Origin origin, string name, string uniqueId, string sensor_category, string stateTopic, string valueTemplate = "{{ value_json.value }}", string? unitOfMeasurement = "", string? icon = "", string? sensorCategory = "", bool isBinarySensor = false, bool? retain = false, int? qos = 1)
    {
        var result = new Config
        {
            Device = device,
            Origin = origin,
            Name = name,
            UniqueId = uniqueId,
            ObjectId = uniqueId,
            StateTopic = stateTopic,
            DeviceClass = sensor_category,
            ValueTemplate = valueTemplate,
            Retain = retain,
            Qos = qos
        };

        if (!string.IsNullOrWhiteSpace(unitOfMeasurement)) result.UnitOfMeasurement = unitOfMeasurement;
        if (!string.IsNullOrWhiteSpace(icon)) result.Icon = icon;
        if (!string.IsNullOrWhiteSpace(sensorCategory)) result.SensorCategory = sensorCategory;
        
        return result;
    }

    /// <summary>
    /// Build config discovery object for a switch/toggle (actor)
    /// </summary>
    /// <param name="device"></param>
    /// <param name="origin"></param>
    /// <param name="name"></param>
    /// <param name="uniqueId"></param>
    /// <param name="stateTopic"></param>
    /// <param name="commandTopic"></param>
    /// <param name="icon"></param>
    /// <param name="valueTemplate"></param>
    /// <param name="retain"></param>
    /// <param name="qos"></param>
    /// <returns></returns>
    public static Config BuildSwitchConfig(Device device, Origin origin, string name, string uniqueId, string stateTopic, string commandTopic, string? icon = "", string? valueTemplate = "{{ value_json.value }}", bool? retain = false, int? qos = 1)
    {
        var result = new Config
        {
            Device = device,
            Origin = origin,
            Name = name,
            UniqueId = uniqueId,
            ObjectId = uniqueId,
            StateTopic = stateTopic,
            CommandTopic = commandTopic,
            Retain = retain,
            Qos = qos,
            ValueTemplate = valueTemplate
        };

        if (!string.IsNullOrWhiteSpace(icon))
        {
            result.Icon = icon;
        }
        return result;
    }

    public static string BuildIdentifier(string name, string type = "config")
    {
        return $"ec_{name.Replace(' ', '-').ToLowerInvariant()}_{type.Replace(' ', '-').ToLowerInvariant()}";
    }

    public static string BuildDeviceCategory(SensorType sensorType)
    {
        return sensorType switch
        {
            SensorType.ApparentPower => "apparent_power",
            SensorType.Battery => "battery",
            SensorType.Aqi => "aqi",
            SensorType.AtmosphericPressure => "atmospheric_pressure",
            SensorType.CarbonDioxide => "carbon_dioxide",
            SensorType.CarbonMonoxide => "carbon_monoxide",
            SensorType.Current => "current",
            SensorType.DataRate => "data_rate",
            SensorType.DataSize => "data_size",
            SensorType.Date => "date",
            SensorType.Distance => "distance",
            SensorType.Duration => "duration",
            SensorType.Energy => "energy",
            SensorType.EnergyStorage => "energy_storage",
            SensorType.Enum => "enum",
            SensorType.Frequency => "frequency",
            SensorType.Gas => "gas",
            SensorType.Humidity => "humidity",
            SensorType.Illuminance => "illuminance",
            SensorType.Irradiance => "irradiance",
            SensorType.Moisture => "moisture",
            SensorType.Monetary => "monetary",
            SensorType.NitrogenDioxide => "nitrogen_dioxide",
            SensorType.NitrogenMonoxide => "nitrogen_monoxide",
            SensorType.NitrousOxide => "nitrous_oxide",
            SensorType.Ozone => "ozone",
            SensorType.Ph => "ph",
            SensorType.Pm1 => "pm1",
            SensorType.Pm25 => "pm25",
            SensorType.Pm10 => "pm10",
            SensorType.PowerFactor => "power_factor",
            SensorType.Power => "power",
            SensorType.Precipitation => "precipitation",
            SensorType.PrecipitationIntensity => "precipitation_intensity",
            SensorType.Pressure => "pressure",
            SensorType.ReactivePower => "reactive_power",
            SensorType.SignalStrength => "signal_strength",
            SensorType.SoundPressure => "sound_pressure",
            SensorType.Speed => "speed",
            SensorType.SulphurDioxide => "sulphur_dioxide",
            SensorType.Temperature => "temperature",
            SensorType.Timestamp => "timestamp",
            SensorType.VolatileOrganicCompounds => "volatile_organic_compounds",
            SensorType.VolatileOrganicCompoundsParts => "volatile_organic_compounds_parts",
            SensorType.Voltage => "voltage",
            SensorType.Volume => "volume",
            SensorType.VolumeFlowRate => "volume_flow_rate",
            SensorType.VolumeStorage => "volume_storage",
            SensorType.Water => "water",
            SensorType.Weight => "weight",
            SensorType.WindSpeed => "wind_speed",
            _ => "none"
        };  
    }
}