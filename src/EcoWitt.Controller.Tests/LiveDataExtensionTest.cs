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

    [Test]
    public void Map_Co2_ProducesAqinSensors()
    {
        var data = LoadFixture("livedata-gw3000-test.json");

        var device = data.Map(isMetric: true, calculateValues: false);

        var co2 = device.Sensors.FirstOrDefault(s => s.Name == "co2");
        Assert.That(co2, Is.Not.Null);

        var pm25 = device.Sensors.FirstOrDefault(s => s.Name == "pm25_co2");
        Assert.That(pm25, Is.Not.Null);
        Assert.That((double)pm25!.Value!, Is.EqualTo(4.0).Within(0.01));

        var pm10 = device.Sensors.FirstOrDefault(s => s.Name == "pm10_co2");
        Assert.That(pm10, Is.Not.Null);
        Assert.That((double)pm10!.Value!, Is.EqualTo(4.4).Within(0.01));

        var temp = device.Sensors.FirstOrDefault(s => s.Name == "tf_co2");
        Assert.That(temp, Is.Not.Null);
        Assert.That((double)temp!.Value!, Is.EqualTo(23.9).Within(0.01));

        var humi = device.Sensors.FirstOrDefault(s => s.Name == "humi_co2");
        Assert.That(humi, Is.Not.Null);

        var batt = device.Sensors.FirstOrDefault(s => s.Name == "co2_batt");
        Assert.That(batt, Is.Not.Null);
    }

    [Test]
    public void Map_Co2_SkipsDoubleDashSentinel()
    {
        var data = LoadFixture("livedata-gw3000-test.json");

        var device = data.Map(isMetric: true, calculateValues: false);

        // PM1_24H and PM4_24H are "--.-" in the fixture — should produce no sensor
        var pm1_24h = device.Sensors.FirstOrDefault(s => s.Name == "pm1_24h_co2");
        Assert.That(pm1_24h, Is.Null);
        var pm4_24h = device.Sensors.FirstOrDefault(s => s.Name == "pm4_24h_co2");
        Assert.That(pm4_24h, Is.Null);
    }

    [Test]
    public void Map_PiezoRain_ProducesRainSensorsAndWs90BatteryFromLastElement()
    {
        var data = LoadFixture("livedata-gw3000-prod.json");

        var device = data.Map(isMetric: true, calculateValues: false);

        // rain rate from 0x0E (0.0 mm/Hr)
        var rainRate = device.Sensors.FirstOrDefault(s => s.Name == "rrain_piezo");
        Assert.That(rainRate, Is.Not.Null, "rain rate sensor missing");

        // monthly rain from 0x12 (22.3 mm)
        var monthlyRain = device.Sensors.FirstOrDefault(s => s.Name == "mrain_piezo");
        Assert.That(monthlyRain, Is.Not.Null, "monthly rain sensor missing");
        Assert.That((double)monthlyRain!.Value!, Is.EqualTo(22.3).Within(0.1));

        // yearly rain from 0x13 (120.3 mm)
        var yearlyRain = device.Sensors.FirstOrDefault(s => s.Name == "yrain_piezo");
        Assert.That(yearlyRain, Is.Not.Null, "yearly rain sensor missing");
        Assert.That((double)yearlyRain!.Value!, Is.EqualTo(120.3).Within(0.1));

        // WS90 battery from the LAST piezoRain element (level 3)
        var ws90Batt = device.Sensors.FirstOrDefault(s => s.Name == "ws90batt");
        Assert.That(ws90Batt, Is.Not.Null, "WS90 battery missing (should source from last piezoRain element)");

        // WS90 capacitor voltage (2.3V)
        var capVolt = device.Sensors.FirstOrDefault(s => s.Name == "ws90cap_volt");
        Assert.That(capVolt, Is.Not.Null, "WS90 capacitor voltage missing");
        Assert.That((double)capVolt!.Value!, Is.EqualTo(2.3).Within(0.01));
    }

    [Test]
    public void Map_CommonList_ProducesOutdoorWeatherSensors()
    {
        var data = LoadFixture("livedata-gw3000-prod.json");

        var device = data.Map(isMetric: true, calculateValues: false);

        var outdoorTemp = device.Sensors.FirstOrDefault(s => s.Name == "tempf");
        Assert.That(outdoorTemp, Is.Not.Null, "outdoor temperature missing (0x02)");
        Assert.That((double)outdoorTemp!.Value!, Is.EqualTo(12.9).Within(0.01));

        var outdoorHumi = device.Sensors.FirstOrDefault(s => s.Name == "humidity");
        Assert.That(outdoorHumi, Is.Not.Null, "outdoor humidity missing (0x07)");

        var windDir = device.Sensors.FirstOrDefault(s => s.Name == "winddir");
        Assert.That(windDir, Is.Not.Null, "wind direction missing (0x0A)");

        var windSpeed = device.Sensors.FirstOrDefault(s => s.Name == "windspeedmph");
        Assert.That(windSpeed, Is.Not.Null, "wind speed missing (0x0B)");

        var windGust = device.Sensors.FirstOrDefault(s => s.Name == "windgustmph");
        Assert.That(windGust, Is.Not.Null, "wind gust missing (0x0C)");

        var solar = device.Sensors.FirstOrDefault(s => s.Name == "solarradiation");
        Assert.That(solar, Is.Not.Null, "solar radiation missing (0x15)");

        var uv = device.Sensors.FirstOrDefault(s => s.Name == "uv");
        Assert.That(uv, Is.Not.Null, "UV missing (0x17)");
    }

    [Test]
    public void Map_CommonList_UnknownIdsAreIgnored()
    {
        var data = new GatewayLiveData
        {
            IpAddress = "192.168.0.1",
            TimestampUtc = DateTime.UtcNow,
            CommonList = new List<CommonListItem>
            {
                new() { Id = "0xFF", Val = "42", Unit = "C" },
                new() { Id = "999", Val = "ignored" }
            }
        };

        var device = data.Map(isMetric: true, calculateValues: false);

        Assert.That(device.Sensors.Count, Is.EqualTo(0), "unknown IDs should produce no sensors");
    }

    [Test]
    public void Map_CommonList_WindInKmH_PassesThrough()
    {
        // Regression: gateway configured to display wind in km/h returned "21.60 km/h".
        // Earlier code applied m/s->km/h conversion (×3.6) yielding 77.76; passthrough must return 21.6.
        var data = new GatewayLiveData
        {
            IpAddress = "192.168.0.1",
            TimestampUtc = DateTime.UtcNow,
            CommonList = new List<CommonListItem>
            {
                new() { Id = "0x19", Val = "21.60 km/h" }
            }
        };

        var device = data.Map(isMetric: true, calculateValues: false);

        var gust = device.Sensors.FirstOrDefault(s => s.Name == "maxdailygust");
        Assert.That(gust, Is.Not.Null);
        Assert.That((double)gust!.Value!, Is.EqualTo(21.6).Within(0.01));
        Assert.That(gust.UnitOfMeasurement, Is.EqualTo("km/h"));
    }

    [Test]
    public void Map_Wh25_NormalizesBareCelsiusUnit()
    {
        var data = new GatewayLiveData
        {
            IpAddress = "192.168.0.1",
            TimestampUtc = DateTime.UtcNow,
            Wh25 = new List<Wh25Reading>
            {
                new() { Intemp = "23.5", Unit = "C", Inhumi = "49%", Abs = "1004.0 hPa", Rel = "1004.0 hPa" }
            }
        };

        var device = data.Map(isMetric: true, calculateValues: false);

        var temp = device.Sensors.FirstOrDefault(s => s.Name == "tempinf");
        Assert.That(temp, Is.Not.Null);
        Assert.That(temp!.UnitOfMeasurement, Is.EqualTo("°C"));
        Assert.That((double)temp.Value!, Is.EqualTo(23.5).Within(0.01));
    }

    [Test]
    public void Map_CommonList_TemperatureInCelsius_NormalizesUnit()
    {
        var data = new GatewayLiveData
        {
            IpAddress = "192.168.0.1",
            TimestampUtc = DateTime.UtcNow,
            CommonList = new List<CommonListItem>
            {
                new() { Id = "0x02", Val = "12.9", Unit = "C" }
            }
        };

        var device = data.Map(isMetric: true, calculateValues: false);

        var temp = device.Sensors.FirstOrDefault(s => s.Name == "tempf");
        Assert.That(temp, Is.Not.Null);
        Assert.That((double)temp!.Value!, Is.EqualTo(12.9).Within(0.01));
        Assert.That(temp.UnitOfMeasurement, Is.EqualTo("°C"));
    }

    [Test]
    public void Map_CommonList_WindInMps_PassesThrough()
    {
        // Wind value embedded as "0.6 m/s" — passthrough yields 0.6 with unit "m/s".
        var data = new GatewayLiveData
        {
            IpAddress = "192.168.0.1",
            TimestampUtc = DateTime.UtcNow,
            CommonList = new List<CommonListItem>
            {
                new() { Id = "0x0B", Val = "0.6 m/s" }
            }
        };

        var device = data.Map(isMetric: true, calculateValues: false);

        var windSpeed = device.Sensors.FirstOrDefault(s => s.Name == "windspeedmph");
        Assert.That(windSpeed, Is.Not.Null);
        Assert.That((double)windSpeed!.Value!, Is.EqualTo(0.6).Within(0.001));
        Assert.That(windSpeed.UnitOfMeasurement, Is.EqualTo("m/s"));
    }

    [Test]
    public void Map_PiezoRain_RainValuesPassThrough()
    {
        // Rain values embedded as "22.3 mm" — passthrough yields 22.3 with unit "mm".
        var data = new GatewayLiveData
        {
            IpAddress = "192.168.0.1",
            TimestampUtc = DateTime.UtcNow,
            PiezoRain = new List<PiezoRainItem>
            {
                new() { Id = "0x12", Val = "22.3 mm" }
            }
        };

        var device = data.Map(isMetric: true, calculateValues: false);

        var rain = device.Sensors.FirstOrDefault(s => s.Name == "mrain_piezo");
        Assert.That(rain, Is.Not.Null);
        Assert.That((double)rain!.Value!, Is.EqualTo(22.3).Within(0.01));
        Assert.That(rain.UnitOfMeasurement, Is.EqualTo("mm"));
    }

    [Test]
    public void Map_PiezoRain_RainInInches_PassesThrough()
    {
        // If gateway is configured for imperial, it sends "0.87 in" — passthrough preserves that.
        var data = new GatewayLiveData
        {
            IpAddress = "192.168.0.1",
            TimestampUtc = DateTime.UtcNow,
            PiezoRain = new List<PiezoRainItem>
            {
                new() { Id = "0x12", Val = "0.87 in" }
            }
        };

        var device = data.Map(isMetric: false, calculateValues: false);

        var rain = device.Sensors.FirstOrDefault(s => s.Name == "mrain_piezo");
        Assert.That(rain, Is.Not.Null);
        Assert.That((double)rain!.Value!, Is.EqualTo(0.87).Within(0.001));
        Assert.That(rain.UnitOfMeasurement, Is.EqualTo("in"));
    }
}
