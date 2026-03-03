using Ecowitt.Controller.Service.Mqtt;

namespace EcoWitt.Controller.Tests;

public class MqttPathBuilderTest
{
    [Test]
    public void BuildMqttGatewayTopic()
    {
        Assert.That(MqttPathBuilder.BuildMqttGatewayTopic("MyGateway"), Is.EqualTo("mygateway"));
    }

    [Test]
    public void BuildMqttGatewaySensorTopic()
    {
        Assert.That(MqttPathBuilder.BuildMqttGatewaySensorTopic("gw1", "Indoor Temperature"),
            Is.EqualTo("gw1/sensors/indoor-temperature"));
    }

    [Test]
    public void BuildMqttGatewayDiagnosticTopic()
    {
        Assert.That(MqttPathBuilder.BuildMqttGatewayDiagnosticTopic("gw1", "WH90 Battery"),
            Is.EqualTo("gw1/diag/wh90-battery"));
    }

    [Test]
    public void BuildMqttSubdeviceTopic()
    {
        Assert.That(MqttPathBuilder.BuildMqttSubdeviceTopic("gw1", "12345"),
            Is.EqualTo("gw1/subdevices/12345"));
    }

    [Test]
    public void BuildMqttSubdeviceSensorTopic()
    {
        Assert.That(MqttPathBuilder.BuildMqttSubdeviceSensorTopic("gw1", "12345", "Water Temperature"),
            Is.EqualTo("gw1/subdevices/12345/sensors/water-temperature"));
    }

    [Test]
    public void BuildMqttSubdeviceDiagnosticTopic()
    {
        Assert.That(MqttPathBuilder.BuildMqttSubdeviceDiagnosticTopic("gw1", "12345", "RSSI"),
            Is.EqualTo("gw1/subdevices/12345/diag/rssi"));
    }

    [Test]
    public void BuildMqttSubdeviceCommandTopic_Specific()
    {
        Assert.That(MqttPathBuilder.BuildMqttSubdeviceCommandTopic("gw1", "12345"),
            Is.EqualTo("gw1/subdevices/12345/cmd"));
    }

    [Test]
    public void BuildMqttSubdeviceCommandTopic_Wildcard()
    {
        Assert.That(MqttPathBuilder.BuildMqttSubdeviceCommandTopic(),
            Is.EqualTo("+/subdevices/+/cmd"));
    }

    [Test]
    public void BuildMqttSubdeviceHACommandTopic_Specific()
    {
        Assert.That(MqttPathBuilder.BuildMqttSubdeviceHACommandTopic("gw1", "12345"),
            Is.EqualTo("gw1/subdevices/12345/cmd/homeassistant"));
    }

    [Test]
    public void BuildMqttSubdeviceHACommandTopic_Wildcard()
    {
        Assert.That(MqttPathBuilder.BuildMqttSubdeviceHACommandTopic(),
            Is.EqualTo("+/subdevices/+/cmd/homeassistant"));
    }

    [Test]
    public void Sanitize_SpacesToHyphens()
    {
        Assert.That(MqttPathBuilder.Sanitize("My Gateway Name"), Is.EqualTo("my-gateway-name"));
    }

    [Test]
    public void Sanitize_ToLowercase()
    {
        Assert.That(MqttPathBuilder.Sanitize("GW2000A"), Is.EqualTo("gw2000a"));
    }

    [Test]
    public void Sanitize_AlreadyClean()
    {
        Assert.That(MqttPathBuilder.Sanitize("gw1/sensors/tempf"), Is.EqualTo("gw1/sensors/tempf"));
    }
}
