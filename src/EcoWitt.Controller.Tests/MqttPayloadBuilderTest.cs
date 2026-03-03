using Ecowitt.Controller.Model;
using Ecowitt.Controller.Service.Mqtt;

namespace EcoWitt.Controller.Tests;

public class MqttPayloadBuilderTest
{
    [Test]
    public void BuildGatewayPayload_FullModel()
    {
        var gw = new Device
        {
            IpAddress = "192.168.1.100",
            Name = "weatherstation-01",
            Model = "GW2000A",
            PASSKEY = "ABC123",
            StationType = "GW2000A_V3.1.3",
            Runtime = 12345,
            Freq = "868M"
        };

        dynamic payload = MqttPayloadBuilder.BuildGatewayPayload(gw);

        Assert.That((string)payload.ip, Is.EqualTo("192.168.1.100"));
        Assert.That((string)payload.name, Is.EqualTo("weatherstation-01"));
        Assert.That((string)payload.model, Is.EqualTo("GW2000A"));
        Assert.That((string)payload.state, Is.EqualTo("online"));
    }

    [Test]
    public void BuildGatewayPayload_NoModel_MinimalPayload()
    {
        var gw = new Device
        {
            IpAddress = "192.168.1.100",
            Name = "weatherstation-01",
            Model = null
        };

        dynamic payload = MqttPayloadBuilder.BuildGatewayPayload(gw);

        Assert.That((string)payload.ip, Is.EqualTo("192.168.1.100"));
        Assert.That((string)payload.name, Is.EqualTo("weatherstation-01"));
        // should not have model property (anonymous type without it)
        var type = payload.GetType();
        Assert.That(type.GetProperty("model"), Is.Null);
    }

    [Test]
    public void BuildGatewayPayload_EmptyModel_MinimalPayload()
    {
        var gw = new Device
        {
            IpAddress = "192.168.1.100",
            Name = "gw1",
            Model = ""
        };

        dynamic payload = MqttPayloadBuilder.BuildGatewayPayload(gw);
        var type = payload.GetType();
        Assert.That(type.GetProperty("model"), Is.Null);
    }

    [Test]
    public void BuildSubdevicePayload_Online()
    {
        var sd = new Subdevice
        {
            Id = 12345,
            Model = SubdeviceModel.WFC01,
            Devicename = "WFC01",
            Nickname = "Garden Valve",
            Availability = true,
            Version = 113
        };

        dynamic payload = MqttPayloadBuilder.BuildSubdevicePayload(sd);

        Assert.That((int)payload.id, Is.EqualTo(12345));
        Assert.That((SubdeviceModel)payload.model, Is.EqualTo(SubdeviceModel.WFC01));
        Assert.That((string)payload.state, Is.EqualTo("online"));
        Assert.That((string)payload.nickname, Is.EqualTo("Garden Valve"));
    }

    [Test]
    public void BuildSubdevicePayload_Offline()
    {
        var sd = new Subdevice
        {
            Id = 99,
            Model = SubdeviceModel.AC1100,
            Availability = false,
            Version = 103
        };

        dynamic payload = MqttPayloadBuilder.BuildSubdevicePayload(sd);
        Assert.That((string)payload.state, Is.EqualTo("offline"));
    }

    [Test]
    public void BuildSensorPayload_DoubleRounding()
    {
        var sensor = new Sensor("tempf", "Outdoor Temperature", 22.456789, SensorDataType.Double, "°C", SensorType.Temperature);

        dynamic payload = MqttPayloadBuilder.BuildSensorPayload(sensor, precision: 2);
        Assert.That((double)payload.value, Is.EqualTo(22.46).Within(0.001));

        dynamic payload1 = MqttPayloadBuilder.BuildSensorPayload(sensor, precision: 1);
        Assert.That((double)payload1.value, Is.EqualTo(22.5).Within(0.01));
    }

    [Test]
    public void BuildSensorPayload_IntegerNotRounded()
    {
        var sensor = new Sensor("uv", 5, SensorDataType.Integer);

        dynamic payload = MqttPayloadBuilder.BuildSensorPayload(sensor, precision: 2);
        Assert.That((int)payload.value, Is.EqualTo(5));
    }

    [Test]
    public void BuildSensorPayload_NullUnitOmitted()
    {
        var sensor = new Sensor("uv", 5, SensorDataType.Integer, unitOfMeasurement: "");

        dynamic payload = MqttPayloadBuilder.BuildSensorPayload(sensor, precision: 2);
        Assert.That((object?)payload.unit, Is.Null);
    }

    [Test]
    public void BuildSensorPayload_UnitIncluded()
    {
        var sensor = new Sensor("tempf", 20.0, SensorDataType.Double, "°C", SensorType.Temperature);

        dynamic payload = MqttPayloadBuilder.BuildSensorPayload(sensor, precision: 2);
        Assert.That((string)payload.unit, Is.EqualTo("°C"));
    }

    [Test]
    public void BuildSensorPayload_IncludesNameAndAlias()
    {
        var sensor = new Sensor("tempinf", "Indoor Temperature", 20.0, SensorDataType.Double, "°C", SensorType.Temperature);

        dynamic payload = MqttPayloadBuilder.BuildSensorPayload(sensor, precision: 2);
        Assert.That((string)payload.name, Is.EqualTo("tempinf"));
        Assert.That((string)payload.alias, Is.EqualTo("Indoor Temperature"));
    }

    [Test]
    public void BuildSensorPayload_DefaultPrecision()
    {
        var sensor = new Sensor("tempf", 22.456789, SensorDataType.Double, "°C", SensorType.Temperature);

        // null precision should default to 2
        dynamic payload = MqttPayloadBuilder.BuildSensorPayload(sensor, precision: null);
        Assert.That((double)payload.value, Is.EqualTo(22.46).Within(0.001));
    }
}
