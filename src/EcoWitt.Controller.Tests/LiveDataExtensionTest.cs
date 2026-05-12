using System.Text.Json;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Mapping;
using Ecowitt.Controller.Model.Message.Data;
using NUnit.Framework;

namespace EcoWitt.Controller.Tests;

[TestFixture]
public class LiveDataExtensionTest
{
    private static GatewayLiveData LoadFixture(string fixtureName)
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", fixtureName);
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<GatewayLiveData>(json);
        Assert.That(data, Is.Not.Null, $"fixture {fixtureName} failed to deserialize");
        data!.IpAddress = "192.168.0.1";
        data.TimestampUtc = new DateTime(2026, 5, 10, 12, 0, 0, DateTimeKind.Utc);
        return data;
    }

    [Test]
    public void Map_Wh25_ProducesIndoorTempHumidityPressureSensors()
    {
        var data = LoadFixture("livedata-gw3000-test.json");

        var device = data.Map(isMetric: true, calculateValues: false);

        var intemp = device.Sensors.FirstOrDefault(s => s.Name == "tempinf");
        Assert.That(intemp, Is.Not.Null, "indoor temp sensor missing");
        Assert.That((double)intemp!.Value!, Is.EqualTo(23.5).Within(0.01));

        var inhumi = device.Sensors.FirstOrDefault(s => s.Name == "humidityin");
        Assert.That(inhumi, Is.Not.Null, "indoor humidity sensor missing");
        Assert.That((double)inhumi!.Value!, Is.EqualTo(49.0).Within(0.01));

        var pressureAbs = device.Sensors.FirstOrDefault(s => s.Name == "baromabsin");
        Assert.That(pressureAbs, Is.Not.Null, "absolute pressure sensor missing");
        Assert.That((double)pressureAbs!.Value!, Is.EqualTo(1004.0).Within(0.1));

        var pressureRel = device.Sensors.FirstOrDefault(s => s.Name == "baromrelin");
        Assert.That(pressureRel, Is.Not.Null, "relative pressure sensor missing");
        Assert.That((double)pressureRel!.Value!, Is.EqualTo(1004.0).Within(0.1));
    }
}
