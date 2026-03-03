using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Api;
using Ecowitt.Controller.Model.Mapping;

namespace EcoWitt.Controller.Tests;

public class ApiDataExtensionTest
{
    private const string GatewayPayload = "[{\"name\":\"tempinf\",\"value\":\"72.5\"},{\"name\":\"humidityin\",\"value\":\"45\"},{\"name\":\"baromrelin\",\"value\":\"29.92\"},{\"name\":\"windspeedmph\",\"value\":\"5.5\"},{\"name\":\"winddir\",\"value\":\"180\"},{\"name\":\"uv\",\"value\":\"3\"}]";

    private const string WFC01Payload = "{\"command\":[{\"model\":1,\"id\":13398,\"nickname\":\"WFC01-00003456\",\"devicename\":\"MJULtW6rvT1I8dEKz3o2\",\"version\":113,\"water_status\":0,\"warning\":0,\"always_on\":0,\"val_type\":1,\"val\":15,\"run_time\":24,\"wfc01batt\":5,\"rssi\":4,\"gw_rssi\":-52,\"timeutc\":1721337849,\"publish_time\":1719251738,\"water_action\":4,\"water_running\":0,\"plan_status\":0,\"water_total\":\"617.557\",\"happen_water\":\"614.258\",\"flow_velocity\":\"0.00\",\"water_temp\":\"18.8\"}]}";

    private const string AC1100Payload = "{\"command\":[{\"model\":2,\"id\":10695,\"nickname\":\"AC1100-000029C7\",\"devicename\":\"xTNGzWMorVwEKqvltP30\",\"version\":103,\"ac_status\":1,\"warning\":0,\"always_on\":1,\"val_type\":1,\"val\":3,\"run_time\":0,\"rssi\":3,\"gw_rssi\":-64,\"timeutc\":1721419571,\"publish_time\":1721419571,\"ac_action\":3,\"ac_running\":1,\"plan_status\":1,\"elect_total\":14821,\"happen_elect\":0,\"realtime_power\":0,\"ac_voltage\":232,\"ac_current\":0}]}";

    [Test]
    public void MapGateway_Metric_ConvertsUnits()
    {
        var data = new GatewayApiData
        {
            PASSKEY = "ABC123",
            Model = "GW2000A",
            StationType = "GW2000A_V3.1.3",
            Runtime = 1000,
            Freq = "868M",
            IpAddress = "192.168.1.100",
            Payload = GatewayPayload
        };

        var device = data.Map(isMetric: true, calculateValues: false);

        Assert.That(device.IpAddress, Is.EqualTo("192.168.1.100"));
        Assert.That(device.Model, Is.EqualTo("GW2000A"));
        Assert.That(device.PASSKEY, Is.EqualTo("ABC123"));
        Assert.That(device.Sensors.Count, Is.GreaterThan(0));

        var temp = device.Sensors.FirstOrDefault(s => s.Name == "tempinf");
        Assert.That(temp, Is.Not.Null);
        Assert.That(temp!.UnitOfMeasurement, Is.EqualTo("°C"));
        // 72.5°F = 22.5°C
        Assert.That((double)temp.Value!, Is.EqualTo(22.5).Within(0.1));
    }

    [Test]
    public void MapGateway_Imperial_NoConversion()
    {
        var data = new GatewayApiData
        {
            Model = "GW2000A",
            Payload = GatewayPayload
        };

        var device = data.Map(isMetric: false, calculateValues: false);

        var temp = device.Sensors.FirstOrDefault(s => s.Name == "tempinf");
        Assert.That(temp, Is.Not.Null);
        Assert.That(temp!.UnitOfMeasurement, Is.EqualTo("F"));
        Assert.That((double)temp.Value!, Is.EqualTo(72.5).Within(0.01));
    }

    [Test]
    public void MapGateway_WithCalculatedValues()
    {
        var data = new GatewayApiData
        {
            Model = "GW2000A",
            Payload = GatewayPayload
        };

        var device = data.Map(isMetric: true, calculateValues: true);

        // should have calculated dewpoint, wind compass etc.
        var compass = device.Sensors.FirstOrDefault(s => s.Name == "winddir-comp");
        Assert.That(compass, Is.Not.Null);
        Assert.That(compass!.Value, Is.EqualTo("S"));
    }

    [Test]
    public void MapGateway_EmptyPayload_NoSensors()
    {
        var data = new GatewayApiData
        {
            Model = "GW2000A",
            Payload = ""
        };

        var device = data.Map();
        Assert.That(device.Sensors, Is.Empty);
    }

    [Test]
    public void MapGateway_OkPayload_NoSensors()
    {
        var data = new GatewayApiData
        {
            Model = "GW2000A",
            Payload = "200 OK"
        };

        var device = data.Map();
        Assert.That(device.Sensors, Is.Empty);
    }

    [Test]
    public void MapSubdevice_WFC01()
    {
        var data = new SubdeviceApiData
        {
            Id = 13398,
            Model = 1,
            Version = 113,
            RfnetState = 1,
            Battery = 5,
            Signal = 4,
            GwIp = "192.168.1.100",
            Payload = WFC01Payload
        };

        var subdevice = data.Map(isMetric: true);

        Assert.That(subdevice.Id, Is.EqualTo(13398));
        Assert.That(subdevice.Model, Is.EqualTo(SubdeviceModel.WFC01));
        Assert.That(subdevice.Availability, Is.True);
        Assert.That(subdevice.Nickname, Is.EqualTo("WFC01-00003456"));
        Assert.That(subdevice.Devicename, Is.EqualTo("MJULtW6rvT1I8dEKz3o2"));
        Assert.That(subdevice.Sensors.Count, Is.GreaterThan(0));

        var waterTotal = subdevice.Sensors.FirstOrDefault(s => s.Name == "water_total");
        Assert.That(waterTotal, Is.Not.Null);
        Assert.That(waterTotal!.SensorType, Is.EqualTo(SensorType.Water));
    }

    [Test]
    public void MapSubdevice_AC1100()
    {
        var data = new SubdeviceApiData
        {
            Id = 10695,
            Model = 2,
            Version = 103,
            RfnetState = 1,
            Battery = 9,
            Signal = 4,
            GwIp = "192.168.1.100",
            Payload = AC1100Payload
        };

        var subdevice = data.Map(isMetric: true);

        Assert.That(subdevice.Id, Is.EqualTo(10695));
        Assert.That(subdevice.Model, Is.EqualTo(SubdeviceModel.AC1100));
        Assert.That(subdevice.Nickname, Is.EqualTo("AC1100-000029C7"));
        Assert.That(subdevice.Sensors.Count, Is.GreaterThan(0));

        var power = subdevice.Sensors.FirstOrDefault(s => s.Name == "realtime_power");
        Assert.That(power, Is.Not.Null);
        Assert.That(power!.SensorType, Is.EqualTo(SensorType.Power));

        var voltage = subdevice.Sensors.FirstOrDefault(s => s.Name == "ac_voltage");
        Assert.That(voltage, Is.Not.Null);
    }

    [Test]
    public void MapSubdevice_Unavailable()
    {
        var data = new SubdeviceApiData
        {
            Id = 99,
            Model = 1,
            RfnetState = 0, // offline
            GwIp = "192.168.1.100",
            Payload = ""
        };

        var subdevice = data.Map();
        Assert.That(subdevice.Availability, Is.False);
    }

    [Test]
    public void MapSubdevice_EmptyPayload()
    {
        var data = new SubdeviceApiData
        {
            Id = 99,
            Model = 1,
            RfnetState = 1,
            GwIp = "192.168.1.100",
            Payload = ""
        };

        var subdevice = data.Map();
        Assert.That(subdevice.Sensors, Is.Empty);
    }

    [Test]
    public void MapSubdevice_WFC01_WithCalculatedValues()
    {
        var data = new SubdeviceApiData
        {
            Id = 13398,
            Model = 1,
            Version = 113,
            RfnetState = 1,
            GwIp = "192.168.1.100",
            Payload = WFC01Payload
        };

        var subdevice = data.Map(isMetric: true, calculateValues: true);

        // happen_water should be recalculated as delta
        var happenWater = subdevice.Sensors.FirstOrDefault(s => s.Name == "happen_water");
        Assert.That(happenWater, Is.Not.Null);
        // water_total (617.557) - happen_water (614.258) = 3.299
        Assert.That((double)happenWater!.Value!, Is.EqualTo(3.299).Within(0.01));
    }
}
