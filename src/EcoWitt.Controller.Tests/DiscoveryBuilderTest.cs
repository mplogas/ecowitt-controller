using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Discovery;

namespace EcoWitt.Controller.Tests;

public class DiscoveryBuilderTest
{
    [Test]
    public void BuildIdentifier_DefaultType()
    {
        var id = DiscoveryBuilder.BuildIdentifier("My Gateway");
        Assert.That(id, Is.EqualTo("ec_my-gateway_config"));
    }

    [Test]
    public void BuildIdentifier_WithType()
    {
        var id = DiscoveryBuilder.BuildIdentifier("weatherstation 01", "availability");
        Assert.That(id, Is.EqualTo("ec_weatherstation-01_availability"));
    }

    [Test]
    public void BuildIdentifier_AlreadyClean()
    {
        var id = DiscoveryBuilder.BuildIdentifier("gw1", "temperature");
        Assert.That(id, Is.EqualTo("ec_gw1_temperature"));
    }

    [Test]
    public void BuildDevice_MinimalNoModel()
    {
        var device = DiscoveryBuilder.BuildDevice("testgw");
        Assert.That(device.Name, Is.EqualTo("testgw"));
        Assert.That(device.Identifiers, Contains.Item("ec_testgw_config"));
        Assert.That(device.Model, Is.Null);
        Assert.That(device.ViaDevice, Is.Null);
    }

    [Test]
    public void BuildDevice_Full()
    {
        var device = DiscoveryBuilder.BuildDevice("MyGW", "GW2000A", "Ecowitt", "GW2000A", "V3.1.3");
        Assert.That(device.Name, Is.EqualTo("MyGW"));
        Assert.That(device.Model, Is.EqualTo("GW2000A"));
        Assert.That(device.Manufacturer, Is.EqualTo("Ecowitt"));
        Assert.That(device.HwVersion, Is.EqualTo("GW2000A"));
        Assert.That(device.SwVersion, Is.EqualTo("V3.1.3"));
    }

    [Test]
    public void BuildDevice_WithViaDevice()
    {
        var device = DiscoveryBuilder.BuildDevice("subdev", "WFC01", "Ecowitt", "WFC01", "113", "ec_gw1_config");
        Assert.That(device.ViaDevice, Is.EqualTo("ec_gw1_config"));
    }

    [Test]
    public void BuildOrigin_Default()
    {
        var origin = DiscoveryBuilder.BuildOrigin();
        Assert.That(origin.Name, Is.EqualTo("Ecowitt Controller"));
        Assert.That(origin.Sw, Is.EqualTo("v2.0.0"));
        Assert.That(origin.Url, Does.Contain("github.com"));
    }

    [Test]
    public void BuildGatewayConfig()
    {
        var device = DiscoveryBuilder.BuildDevice("gw1");
        var origin = DiscoveryBuilder.BuildOrigin();
        var config = DiscoveryBuilder.BuildGatewayConfig(device, origin, "Availability", "ec_gw1_availability", "ecowitt/gw1/availability", "ecowitt/gw1/availability");

        Assert.That(config.Name, Is.EqualTo("Availability"));
        Assert.That(config.UniqueId, Is.EqualTo("ec_gw1_availability"));
        Assert.That(config.ObjectId, Is.EqualTo("ec_gw1_availability"));
        Assert.That(config.StateTopic, Is.EqualTo("ecowitt/gw1/availability"));
        Assert.That(config.Qos, Is.EqualTo(1));
        Assert.That(config.Retain, Is.False);
    }

    [Test]
    public void BuildSensorConfig()
    {
        var device = DiscoveryBuilder.BuildDevice("gw1");
        var origin = DiscoveryBuilder.BuildOrigin();
        var config = DiscoveryBuilder.BuildSensorConfig(device, origin, "Indoor Temperature", "ec_gw1_tempinf_temperature", "temperature", "ecowitt/gw1/sensors/indoor-temperature", unitOfMeasurement: "°C");

        Assert.That(config.Name, Is.EqualTo("Indoor Temperature"));
        Assert.That(config.DeviceClass, Is.EqualTo("temperature"));
        Assert.That(config.UnitOfMeasurement, Is.EqualTo("°C"));
        Assert.That(config.ValueTemplate, Is.EqualTo("{{ value_json.value }}"));
    }

    [Test]
    public void BuildSensorConfig_BinarySensor()
    {
        var device = DiscoveryBuilder.BuildDevice("gw1");
        var origin = DiscoveryBuilder.BuildOrigin();
        var config = DiscoveryBuilder.BuildSensorConfig(device, origin, "Rain State", "ec_gw1_rain", "none", "ecowitt/gw1/sensors/rain", isBinarySensor: true);

        Assert.That(config, Is.Not.Null);
    }

    [Test]
    public void BuildSensorConfig_DiagnosticCategory()
    {
        var device = DiscoveryBuilder.BuildDevice("gw1");
        var origin = DiscoveryBuilder.BuildOrigin();
        var config = DiscoveryBuilder.BuildSensorConfig(device, origin, "RSSI", "ec_gw1_rssi", "signal_strength", "ecowitt/gw1/diag/rssi", sensorCategory: "diagnostic");

        Assert.That(config.SensorCategory, Is.EqualTo("diagnostic"));
    }

    [Test]
    public void BuildSwitchConfig()
    {
        var device = DiscoveryBuilder.BuildDevice("valve1");
        var origin = DiscoveryBuilder.BuildOrigin();
        var config = DiscoveryBuilder.BuildSwitchConfig(device, origin, "switch", "ec_valve1_switch", "ecowitt/gw1/subdevices/123/diag/running", "ecowitt/gw1/subdevices/123/cmd/homeassistant");

        Assert.That(config.CommandTopic, Is.EqualTo("ecowitt/gw1/subdevices/123/cmd/homeassistant"));
        Assert.That(config.StateTopic, Does.Contain("running"));
    }

    // Device class mapping
    [TestCase(SensorType.Temperature, "temperature")]
    [TestCase(SensorType.Humidity, "humidity")]
    [TestCase(SensorType.Pressure, "pressure")]
    [TestCase(SensorType.Battery, "battery")]
    [TestCase(SensorType.WindSpeed, "wind_speed")]
    [TestCase(SensorType.Precipitation, "precipitation")]
    [TestCase(SensorType.PrecipitationIntensity, "precipitation_intensity")]
    [TestCase(SensorType.Voltage, "voltage")]
    [TestCase(SensorType.Current, "current")]
    [TestCase(SensorType.Power, "power")]
    [TestCase(SensorType.Energy, "energy")]
    [TestCase(SensorType.SignalStrength, "signal_strength")]
    [TestCase(SensorType.CarbonDioxide, "carbon_dioxide")]
    [TestCase(SensorType.Pm25, "pm25")]
    [TestCase(SensorType.Pm10, "pm10")]
    [TestCase(SensorType.Pm1, "pm1")]
    [TestCase(SensorType.Distance, "distance")]
    [TestCase(SensorType.Irradiance, "irradiance")]
    [TestCase(SensorType.Moisture, "moisture")]
    [TestCase(SensorType.Water, "water")]
    [TestCase(SensorType.VolumeFlowRate, "volume_flow_rate")]
    [TestCase(SensorType.None, "none")]
    public void BuildDeviceCategory_MapsCorrectly(SensorType type, string expected)
    {
        Assert.That(DiscoveryBuilder.BuildDeviceCategory(type), Is.EqualTo(expected));
    }
}
