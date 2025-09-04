

using Newtonsoft.Json;

namespace Ecowitt.Controller.Model;

public enum SensorDataType
{
    Integer,
    Double,
    Boolean,
    String,
    DateTime
}

public interface ISensor
{
    string Name { get; }
    string Alias { get; }
    DateTime TimestampUtc { get; }
    SensorType SensorType { get; }
    SensorState SensorState { get; }
    SensorClass SensorClass { get; }
    SensorCategory SensorCategory { get; }
    string UnitOfMeasurement { get; }
    SensorDataType DataType { get; }
    object? Value { get; set; }
    bool DiscoveryUpdate { get; set; }
    bool HasChanged { get; }
    void ResetChangeFlag();
}

public sealed class Sensor : ISensor
{
    private int _lastValueHash;
    private object? _value;

    public string Name { get; }
    public string Alias { get; }
    public DateTime TimestampUtc { get; private set; }
    public SensorType SensorType { get; }
    public SensorState SensorState { get; }
    public SensorClass SensorClass { get; }
    public SensorCategory SensorCategory { get; }
    public string UnitOfMeasurement { get; }
    public SensorDataType DataType { get; }
    public bool DiscoveryUpdate { get; set; }
    public bool HasChanged { get; private set; }

    public object? Value
    {
        get => _value;
        set
        {
            var newHash = value?.GetHashCode() ?? 0;
            HasChanged = _lastValueHash != newHash;
            _lastValueHash = newHash;
            _value = value;
            if (HasChanged)
            {
                TimestampUtc = DateTime.UtcNow;
            }
        }
    }

    [JsonConstructor]
    private Sensor(string name,
        string alias,
        object? value,
        SensorDataType dataType,
        string unitOfMeasurement,
        SensorType sensorType,
        SensorState sensorState,
        SensorClass sensorClass,
        SensorCategory sensorCategory,
        bool discoveryUpdate,
        DateTime timestampUtc)
    {
        Name = name;
        Alias = alias;
        DataType = dataType;
        SensorType = sensorType;
        SensorState = sensorState;
        SensorClass = sensorClass;
        SensorCategory = sensorCategory;
        UnitOfMeasurement = unitOfMeasurement;
        DiscoveryUpdate = discoveryUpdate;
        TimestampUtc = timestampUtc == default ? DateTime.UtcNow : DateTime.SpecifyKind(timestampUtc, DateTimeKind.Utc);

        // set value & hash without flagging change
        _value = CoerceValue(value, dataType);
        _lastValueHash = _value?.GetHashCode() ?? 0;
        HasChanged = false;
    }

    public Sensor(string name,
                  string alias,
                  object? value,
                  SensorDataType dataType,
                  string unitOfMeasurement = "",
                  SensorType sensorType = SensorType.None,
                  SensorState sensorState = SensorState.Measurement,
                  SensorClass sensorClass = SensorClass.Sensor,
                  SensorCategory sensorCategory = SensorCategory.Config)
    {
        Name = name;
        Alias = alias;
        DataType = dataType;
        SensorType = sensorType;
        SensorState = sensorState;
        SensorClass = sensorClass;
        SensorCategory = sensorCategory;
        UnitOfMeasurement = unitOfMeasurement;
        TimestampUtc = DateTime.UtcNow;
        Value = value; // invokes setter => sets hash & timestamp
    }

    public Sensor(string name,
                  object? value,
                  SensorDataType dataType,
                  string unitOfMeasurement = "",
                  SensorType sensorType = SensorType.None,
                  SensorState sensorState = SensorState.Measurement,
                  SensorClass sensorClass = SensorClass.Sensor,
                  SensorCategory sensorCategory = SensorCategory.Config)
        : this(name, name, value, dataType, unitOfMeasurement, sensorType, sensorState, sensorClass, sensorCategory)
    { }

    public void ResetChangeFlag() => HasChanged = false;

    // Helper typed accessors (optional usage)
    public double AsDouble() => DataType switch
    {
        SensorDataType.Double => Convert.ToDouble(Value),
        SensorDataType.Integer => Convert.ToDouble(Value),
        _ => throw new InvalidCastException($"Sensor {Name} value is not numeric")
    };
    public int AsInt() => DataType == SensorDataType.Integer ? Convert.ToInt32(Value) : (int)Math.Round(AsDouble());
    public bool AsBool() => DataType == SensorDataType.Boolean ? (bool)(Value ?? false) : throw new InvalidCastException($"Sensor {Name} value is not boolean");
    public string AsString() => Value?.ToString() ?? string.Empty;
    public DateTime AsDateTime() => DataType == SensorDataType.DateTime ? (DateTime)(Value ?? DateTime.MinValue) : throw new InvalidCastException($"Sensor {Name} value is not DateTime");

    private static object? CoerceValue(object? raw, SensorDataType dt)
    {
        if (raw == null) return null;
        try
        {
            return dt switch
            {
                SensorDataType.Integer => Convert.ToInt32(raw),
                SensorDataType.Double => Convert.ToDouble(raw),
                SensorDataType.Boolean => Convert.ToBoolean(raw),
                SensorDataType.String => raw.ToString(),
                SensorDataType.DateTime => raw is DateTime dtv
                    ? DateTime.SpecifyKind(dtv, DateTimeKind.Utc)
                    : DateTime.SpecifyKind(Convert.ToDateTime(raw), DateTimeKind.Utc),
                _ => raw
            };
        }
        catch
        {
            return raw; // fallback, avoid hard failure
        }
    }
}

/// <summary>
/// from: https://www.home-assistant.io/integrations/sensor/#device-class
/// The following device classes are supported for sensors:
///
///    None: Generic sensor. This is the default and doesn’t need to be set.
///    apparent_power: Apparent power in VA.
///    aqi: Air Quality Index (unitless).
///    atmospheric_pressure: Atmospheric pressure in cbar, bar, hPa, mmHg, inHg, kPa, mbar, Pa or psi
///    battery: Percentage of battery that is left in %
///    carbon_dioxide: Carbon Dioxide in CO2 (Smoke) in ppm
///    carbon_monoxide: Carbon Monoxide in CO (Gas CNG/LPG) in ppm
///    current: Current in A, mA
///    data_rate: Data rate in bit/s, kbit/s, Mbit/s, Gbit/s, B/s, kB/s, MB/s, GB/s, KiB/s, MiB/s or GiB/s
///    data_size: Data size in bit, kbit, Mbit, Gbit, B, kB, MB, GB, TB, PB, EB, ZB, YB, KiB, MiB, GiB, TiB, PiB, EiB, ZiB or YiB
///    date: Date string (ISO 8601)
///    distance: Generic distance in km, m, cm, mm, mi, yd, or in
///    duration: Duration in d, h, min, or s
///    energy: Energy in Wh, kWh, MWh, MJ, or GJ
///    energy_storage: Stored energy in Wh, kWh, MWh, MJ, or GJ
///    enum: Has a limited set of (non-numeric) states
///    frequency: Frequency in Hz, kHz, MHz, or GHz
///    gas: Gasvolume in m³, ft³ or CCF
///    humidity: Percentage of humidity in the air in %
///    illuminance: The current light level in lx
///    irradiance: Irradiance in W/m² or BTU/(h⋅ft²)
///    moisture: Percentage of water in a substance in %
///    monetary: The monetary value (ISO 4217)
///    nitrogen_dioxide: Concentration of Nitrogen Dioxide in µg/m³
///    nitrogen_monoxide: Concentration of Nitrogen Monoxide in µg/m³
///    nitrous_oxide: Concentration of Nitrous Oxide in µg/m³
///    ozone: Concentration of Ozone in µg/m³
///    ph: Potential hydrogen (pH) value of a water solution
///    pm1: Concentration of particulate matter less than 1 micrometer in µg/m³
///    pm25: Concentration of particulate matter less than 2.5 micrometers in µg/m³
///    pm10: Concentration of particulate matter less than 10 micrometers in µg/m³
///    power_factor: Power factor (unitless), unit may be None or %
///    power: Power in W or kW
///    precipitation: Accumulated precipitation in cm, in or mm
///    precipitation_intensity: Precipitation intensity in in/d, in/h, mm/d or mm/h
///    pressure: Pressure in Pa, kPa, hPa, bar, cbar, mbar, mmHg, inHg or psi
///    reactive_power: Reactive power in var
///    signal_strength: Signal strength in dB or dBm
///    sound_pressure: Sound pressure in dB or dBA
///    speed: Generic speed in ft/s, in/d, in/h, km/h, kn, m/s, mph or mm/d
///    sulphur_dioxide: Concentration of sulphur dioxide in µg/m³
///    temperature: Temperature in °C, °F or K
///    timestamp: Datetime object or timestamp string (ISO 8601)
///    volatile_organic_compounds: Concentration of volatile organic compounds in µg/m³
///    volatile_organic_compounds_parts: Ratio of volatile organic compounds in ppm or ppb
///    voltage: Voltage in V, mV
///    volume: Generic volume in L, mL, gal, fl. oz., m³, ft³, or CCF
///    volume_flow_rate: Volume flow rate in m³/h, ft³/min, L/min, gal/min
///    volume_storage: Generic stored volume in L, mL, gal, fl. oz., m³, ft³, or CCF
///    water: Water consumption in L, gal, m³, ft³, or CCF
///    weight: Generic mass in kg, g, mg, µg, oz, lb, or st
///    wind_speed: Wind speed in Beaufort, ft/s, km/h, kn, m/s, or mph
/// </summary>
public enum SensorType
{
    None,
    ApparentPower,
    Aqi,
    AtmosphericPressure,
    Battery,
    CarbonDioxide,
    CarbonMonoxide,
    Current,
    DataRate,
    DataSize,
    Date,
    Distance,
    Duration,
    Energy,
    EnergyStorage,
    Enum,
    Frequency,
    Gas,
    Humidity,
    Illuminance,
    Irradiance,
    Moisture,
    Monetary,
    NitrogenDioxide,
    NitrogenMonoxide,
    NitrousOxide,
    Ozone,
    Ph,
    Pm1,
    Pm25,
    Pm10,
    PowerFactor,
    Power,
    Precipitation,
    PrecipitationIntensity,
    Pressure,
    ReactivePower,
    SignalStrength,
    SoundPressure,
    Speed,
    SulphurDioxide,
    Temperature,
    Timestamp,
    VolatileOrganicCompounds,
    VolatileOrganicCompoundsParts,
    Voltage,
    Volume,
    VolumeFlowRate,
    VolumeStorage,
    Water,
    Weight,
    WindSpeed
}

/// <summary>
/// from: https://developers.home-assistant.io/blog/2021/09/20/state_class_total/
/// There are 3 defined state classes:
/// measurement: the state represents a measurement in present time, for example a temperature, electric power, etc. For supported sensors, statistics of min, max and average sensor readings are updated periodically.
/// total: the state represents a total amount that can both increase and decrease, e.g. a net energy meter. When supported, the accumulated growth or decline of the sensor's value since it was first added is updated periodically.
/// total_increasing: a monotonically increasing total, e.g. an amount of consumed gas, water or energy. When supported, the accumulated growth of the sensor's value since it was first added is updated periodically.
/// </summary>
public enum SensorState
{
    Measurement,
    TotalIncreasing,
    Total
}

public enum SensorClass
{
    Sensor,
    BinarySensor
}

/// <summary>
/// From: https://developers.home-assistant.io/docs/core/entity/#registry-properties
/// Classification of a non-primary entity.
/// Set to EntityCategory.CONFIG for an entity that allows changing the configuration of a device,
/// for example, a switch entity, making it possible to turn the background illumination of a switch on and off.
/// Set to EntityCategory.DIAGNOSTIC for an entity exposing some configuration parameter or diagnostics
/// of a device but does not allow changing it, for example, a sensor showing RSSI or MAC address.
/// </summary>
public enum SensorCategory
{
    Config,
    Diagnostic
}