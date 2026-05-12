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

    [Test]
    public void Map_ChSoil_ProducesSoilMoistureVoltageBatteryPerChannel()
    {
        var data = LoadFixture("livedata-gw3000-prod.json");

        var device = data.Map(isMetric: true, calculateValues: false);

        // 16 channels, each producing moisture + battery + voltage = 48 soil sensors
        var soilSensors = device.Sensors.Where(s => s.Name.StartsWith("soil")).ToList();
        Assert.That(soilSensors.Count, Is.EqualTo(48), "expected 48 soil sensors (16 channels x 3 fields)");

        var ch16Moisture = device.Sensors.FirstOrDefault(s => s.Name == "soilmoisture16");
        Assert.That(ch16Moisture, Is.Not.Null);
        Assert.That((double)ch16Moisture!.Value!, Is.EqualTo(43.0).Within(0.01));

        var ch16Voltage = device.Sensors.FirstOrDefault(s => s.Name == "soilbattvolt16");
        Assert.That(ch16Voltage, Is.Not.Null);
        Assert.That((double)ch16Voltage!.Value!, Is.EqualTo(1.60).Within(0.01));
    }

    [Test]
    public void Map_ChSoil_SkipsChannelWithDoubleDashHumidity()
    {
        var data = LoadFixture("livedata-gw3000-test.json");

        var device = data.Map(isMetric: true, calculateValues: false);

        // gw3000-test channel 3 has humidity "--" sentinel — should be skipped
        var ch3Moisture = device.Sensors.FirstOrDefault(s => s.Name == "soilmoisture3");
        Assert.That(ch3Moisture, Is.Null, "channel 3 has '--' humidity; should be skipped");

        var ch8Moisture = device.Sensors.FirstOrDefault(s => s.Name == "soilmoisture8");
        Assert.That(ch8Moisture, Is.Not.Null);
        Assert.That((double)ch8Moisture!.Value!, Is.EqualTo(57.0).Within(0.01));
    }

    [Test]
    public void Map_ChTemp_ProducesTempBatteryVoltagePerChannel()
    {
        var data = LoadFixture("livedata-gw3000-prod.json");

        var device = data.Map(isMetric: true, calculateValues: false);

        var ch1Temp = device.Sensors.FirstOrDefault(s => s.Name == "temp1f");
        Assert.That(ch1Temp, Is.Not.Null, "channel 1 temp sensor missing");
        Assert.That((double)ch1Temp!.Value!, Is.EqualTo(14.0).Within(0.01));

        var ch1Batt = device.Sensors.FirstOrDefault(s => s.Name == "batt1");
        Assert.That(ch1Batt, Is.Not.Null, "channel 1 battery sensor missing");

        var ch1Volt = device.Sensors.FirstOrDefault(s => s.Name == "battvolt1");
        Assert.That(ch1Volt, Is.Not.Null, "channel 1 battery voltage missing");
        Assert.That((double)ch1Volt!.Value!, Is.EqualTo(1.46).Within(0.01));
    }

    [Test]
    public void Map_Lightning_ProducesDistanceCountBatterySensors()
    {
        var data = LoadFixture("livedata-gw3000-prod.json");

        var device = data.Map(isMetric: true, calculateValues: false);

        var distance = device.Sensors.FirstOrDefault(s => s.Name == "lightning");
        Assert.That(distance, Is.Not.Null);
        Assert.That((double)distance!.Value!, Is.EqualTo(8.0).Within(0.01));

        var count = device.Sensors.FirstOrDefault(s => s.Name == "lightning_num");
        Assert.That(count, Is.Not.Null);

        var battery = device.Sensors.FirstOrDefault(s => s.Name == "wh57batt");
        Assert.That(battery, Is.Not.Null);
    }
}
