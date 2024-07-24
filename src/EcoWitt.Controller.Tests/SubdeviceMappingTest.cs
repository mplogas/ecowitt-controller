using System.Diagnostics;
using Ecowitt.Controller.Mapping;
using Ecowitt.Controller.Model;
using Newtonsoft.Json;

namespace EcoWitt.Controller.Tests;

public class SubdeviceMappingTest
{
    private const string json_overview = "{\"command\":[{\"cmd\":\"read_quick\",\"model\":1,\"id\":13398,\"ver\":113,\"rfnet_state\":1,\"battery\":5,\"signal\":4},{\"cmd\":\"read_quick\",\"model\":2,\"id\":10695,\"ver\":103,\"rfnet_state\":1,\"battery\":9,\"signal\":4}]}";
    private const string json_device_ac = "{\"command\":[{\"model\":2,\"id\":10695,\"nickname\":\"AC1100-000029C7\",\"devicename\":\"xTNGzWMorVwEKqvltP30\",\"version\":103,\"ac_status\":1,\"warning\":0,\"always_on\":1,\"val_type\":1,\"val\":3,\"run_time\":0,\"rssi\":3,\"gw_rssi\":-64,\"timeutc\":1721419571,\"publish_time\":1721419571,\"ac_action\":3,\"ac_running\":1,\"plan_status\":1,\"elect_total\":14821,\"happen_elect\":0,\"realtime_power\":0,\"ac_voltage\":232,\"ac_current\":0}]}";
    private const string json_device_wfc = "{\"command\":[{\"model\":1,\"id\":13398,\"nickname\":\"WFC01-00003456\",\"devicename\":\"MJULtW6rvT1I8dEKz3o2\",\"version\":113,\"water_status\":0,\"warning\":0,\"always_on\":0,\"val_type\":1,\"val\":15,\"run_time\":24,\"wfc01batt\":5,\"rssi\":4,\"gw_rssi\":-52,\"timeutc\":1721337849,\"publish_time\":1719251738,\"water_action\":4,\"water_running\":0,\"plan_status\":0,\"water_total\":\"617.557\",\"happen_water\":\"614.258\",\"flow_velocity\":\"0.00\",\"water_temp\":\"18.8\"}]}";

    private List<SubdeviceApiData> subdevices = new();
    
    [SetUp]
    public void Setup()
    {
        
    }

    [Test]
    public void TestOverviewCreation()
    {
        this.subdevices.Clear();
        dynamic? data = JsonConvert.DeserializeObject(json_overview);
        foreach (var device in data.command)
        {
            var subdevice = new SubdeviceApiData
            {
                Id = device.id,
                Model = device.model,
                Version = device.ver,
                RfnetState = device.rfnet_state,
                Battery = device.battery,
                Signal = device.signal,
                GwIp = "127.0.0.1",
                TimestampUtc = DateTime.UtcNow
            };
            subdevices.Add(subdevice);
        }
        
        Assert.That(subdevices.Count, Is.EqualTo(2));
    }

    [Test]
    public void TestDeviceAcCreation()
    {
        this.subdevices.Clear();
        dynamic? data = JsonConvert.DeserializeObject(json_overview);
        foreach (var device in data.command)
        {
            var subdevice = new SubdeviceApiData
            {
                Id = device.id,
                Model = device.model,
                Version = device.ver,
                RfnetState = device.rfnet_state,
                Battery = device.battery,
                Signal = device.signal,
                GwIp = "127.0.0.1",
                TimestampUtc = DateTime.UtcNow,
                Payload = device.model == 2 ? json_device_ac : json_device_wfc
            };
            subdevices.Add(subdevice);
        }
        Assert.That(subdevices, Has.Count.EqualTo(2));
        
        var ac = subdevices.FirstOrDefault(sd => sd.Model == 2);
        Assert.That(ac.Payload, Is.EqualTo(json_device_ac));

        var subDevice = ac.Map();
        
        Assert.That(subDevice.Id, Is.EqualTo(10695));
        Assert.That(subDevice.Model, Is.EqualTo(SubdeviceModel.AC1100));
        Assert.That(subDevice.Version, Is.EqualTo(103));
        Assert.That(subDevice.Devicename, Is.EqualTo("xTNGzWMorVwEKqvltP30"));
        Assert.That(subDevice.Nickname, Is.EqualTo("AC1100-000029C7"));
        Assert.That(subDevice.Availability, Is.EqualTo(true));
        Assert.That(subDevice.Sensors, Has.Count.EqualTo(18));
    }

    [Test]
    public void TestDeviceWfcCreation()
    {
        this.subdevices.Clear();
        dynamic? data = JsonConvert.DeserializeObject(json_overview);
        foreach (var device in data.command)
        {
            var subdevice = new SubdeviceApiData
            {
                Id = device.id,
                Model = device.model,
                Version = device.ver,
                RfnetState = device.rfnet_state,
                Battery = device.battery,
                Signal = device.signal,
                GwIp = "127.0.0.1",
                TimestampUtc = DateTime.UtcNow,
                Payload = device.model == 2 ? json_device_ac : json_device_wfc
            };
            subdevices.Add(subdevice);
        }

        Assert.That(subdevices, Has.Count.EqualTo(2));
        
        var wfc = subdevices.FirstOrDefault(sd => sd.Model == 1);
        Assert.That(wfc.Payload, Is.EqualTo(json_device_wfc));
        
        var subDevice = wfc.Map();
        
        Assert.That(subDevice.Id, Is.EqualTo(13398));
        Assert.That(subDevice.Model, Is.EqualTo(SubdeviceModel.WFC01));
        Assert.That(subDevice.Version, Is.EqualTo(113));
        Assert.That(subDevice.Devicename, Is.EqualTo("MJULtW6rvT1I8dEKz3o2"));
        Assert.That(subDevice.Nickname, Is.EqualTo("WFC01-00003456"));
        Assert.That(subDevice.Availability, Is.EqualTo(true));
        Assert.That(subDevice.Sensors, Has.Count.EqualTo(18));
    }
}