using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Mapping;

namespace EcoWitt.Controller.Tests;

public class SensorBuilderAddonTest
{
    [Test]
    public void CalculateGatewayAddons_DewpointMetric()
    {
        var device = new Device { IpAddress = "1.2.3.4" };
        device.Sensors.Add(new Sensor("tempinf", "Indoor Temperature", 20.0, SensorDataType.Double, "°C", SensorType.Temperature));
        device.Sensors.Add(new Sensor("humidityin", "Indoor Humidity", 50.0, SensorDataType.Double, "%", SensorType.Humidity));
        device.Sensors.Add(new Sensor("tempf", "Outdoor Temperature", 25.0, SensorDataType.Double, "°C", SensorType.Temperature));
        device.Sensors.Add(new Sensor("humidity", "Outdoor Humidity", 60.0, SensorDataType.Double, "%", SensorType.Humidity));
        device.Sensors.Add(new Sensor("windspeedmph", "Wind Speed", 15.0, SensorDataType.Double, "km/h", SensorType.WindSpeed));
        device.Sensors.Add(new Sensor("winddir", "Wind Direction", 180, SensorDataType.Integer, "°"));

        SensorBuilder.CalculateGatewayAddons(ref device, isMetric: true);

        var indoorDewpoint = device.Sensors.FirstOrDefault(s => s.Name == "dewpointin");
        Assert.That(indoorDewpoint, Is.Not.Null);
        Assert.That(indoorDewpoint!.SensorType, Is.EqualTo(SensorType.Temperature));
        // dewpoint = temp - (100 - humidity) / 5 = 20 - (100-50)/5 = 20 - 10 = 10
        Assert.That((double)indoorDewpoint.Value!, Is.EqualTo(10.0).Within(0.5));

        var outdoorDewpoint = device.Sensors.FirstOrDefault(s => s.Name == "dewpoint");
        Assert.That(outdoorDewpoint, Is.Not.Null);
        // 25 - (100-60)/5 = 25 - 8 = 17
        Assert.That((double)outdoorDewpoint!.Value!, Is.EqualTo(17.0).Within(0.5));
    }

    [Test]
    public void CalculateGatewayAddons_HeatIndex()
    {
        var device = new Device { IpAddress = "1.2.3.4" };
        device.Sensors.Add(new Sensor("tempf", "Outdoor Temperature", 30.0, SensorDataType.Double, "°C", SensorType.Temperature));
        device.Sensors.Add(new Sensor("humidity", "Outdoor Humidity", 70.0, SensorDataType.Double, "%", SensorType.Humidity));

        SensorBuilder.CalculateGatewayAddons(ref device, isMetric: true);

        var heatIndex = device.Sensors.FirstOrDefault(s => s.Name == "heatindex");
        Assert.That(heatIndex, Is.Not.Null);
        Assert.That(heatIndex!.SensorType, Is.EqualTo(SensorType.Temperature));
    }

    [Test]
    public void CalculateGatewayAddons_WindChill()
    {
        var device = new Device { IpAddress = "1.2.3.4" };
        device.Sensors.Add(new Sensor("tempf", "Outdoor Temperature", 5.0, SensorDataType.Double, "°C", SensorType.Temperature));
        device.Sensors.Add(new Sensor("humidity", "Outdoor Humidity", 50.0, SensorDataType.Double, "%", SensorType.Humidity));
        device.Sensors.Add(new Sensor("windspeedmph", "Wind Speed", 20.0, SensorDataType.Double, "km/h", SensorType.WindSpeed));

        SensorBuilder.CalculateGatewayAddons(ref device, isMetric: true);

        var windChill = device.Sensors.FirstOrDefault(s => s.Name == "windchill");
        Assert.That(windChill, Is.Not.Null);
        Assert.That(windChill!.SensorType, Is.EqualTo(SensorType.Temperature));
    }

    [Test]
    public void CalculateGatewayAddons_WindDirectionCompass()
    {
        var device = new Device { IpAddress = "1.2.3.4" };
        device.Sensors.Add(new Sensor("winddir", "Wind Direction", 180, SensorDataType.Integer, "°"));

        SensorBuilder.CalculateGatewayAddons(ref device, isMetric: true);

        var compass = device.Sensors.FirstOrDefault(s => s.Name == "winddir-comp");
        Assert.That(compass, Is.Not.Null);
        Assert.That(compass!.Value, Is.EqualTo("S"));
    }

    [TestCase(0, "N")]
    [TestCase(10, "N")]
    [TestCase(11, "NNE")]
    [TestCase(45, "NE")]
    [TestCase(90, "E")]
    [TestCase(135, "SE")]
    [TestCase(180, "S")]
    [TestCase(225, "SW")]
    [TestCase(270, "W")]
    [TestCase(315, "NW")]
    [TestCase(349, "N")]
    [TestCase(359, "N")]
    public void CalculateGatewayAddons_WindCompassBoundaries(int degrees, string expected)
    {
        var device = new Device { IpAddress = "1.2.3.4" };
        device.Sensors.Add(new Sensor("winddir", "Wind Direction", degrees, SensorDataType.Integer, "°"));

        SensorBuilder.CalculateGatewayAddons(ref device, isMetric: true);

        var compass = device.Sensors.FirstOrDefault(s => s.Name == "winddir-comp");
        Assert.That(compass, Is.Not.Null);
        Assert.That(compass!.Value, Is.EqualTo(expected));
    }

    [Test]
    public void CalculateGatewayAddons_PM25AQI()
    {
        var device = new Device { IpAddress = "1.2.3.4" };
        device.Sensors.Add(new Sensor("pm25_24h_co2", "CO2 PM2.5 24h Average", 5.0, SensorDataType.Double, "µg/m³", SensorType.Pm25, SensorState.Total));

        SensorBuilder.CalculateGatewayAddons(ref device, isMetric: true);

        var aqi = device.Sensors.FirstOrDefault(s => s.Name == "pm25_24h_co2-aqi");
        Assert.That(aqi, Is.Not.Null);
        Assert.That(aqi!.Value, Is.EqualTo("Good"));
    }

    [Test]
    public void CalculateGatewayAddons_MissingSensors_NoError()
    {
        var device = new Device { IpAddress = "1.2.3.4" };
        // no sensors at all
        Assert.DoesNotThrow(() => SensorBuilder.CalculateGatewayAddons(ref device, isMetric: true));
        // only indoor temp, no humidity — shouldn't add dewpoint
        device.Sensors.Add(new Sensor("tempinf", "Indoor Temperature", 20.0, SensorDataType.Double, "°C", SensorType.Temperature));
        SensorBuilder.CalculateGatewayAddons(ref device, isMetric: true);
        Assert.That(device.Sensors.FirstOrDefault(s => s.Name == "dewpointin"), Is.Null);
    }

    [Test]
    public void CalculateWFC01Addons_WaterDelta()
    {
        var subdevice = new Subdevice
        {
            Id = 1,
            Model = SubdeviceModel.WFC01,
            GwIp = "1.2.3.4"
        };
        subdevice.Sensors.Add(new Sensor("water_total", "Total Water", 1000.0, SensorDataType.Double, "L", SensorType.Water, SensorState.TotalIncreasing));
        subdevice.Sensors.Add(new Sensor("happen_water", "Last Planned Consumption", 800.0, SensorDataType.Double, "L", SensorType.Water));

        SensorBuilder.CalculateWFC01Addons(ref subdevice);

        var happen = subdevice.Sensors.First(s => s.Name == "happen_water");
        // delta: 1000 - 800 = 200... wait, it replaces happen_water with (total - happen)
        // so: 1000 - 800 = 200
        Assert.That((double)happen.Value!, Is.EqualTo(200.0).Within(0.01));
    }

    [Test]
    public void CalculateWFC01Addons_NonWFC01_Ignored()
    {
        var subdevice = new Subdevice
        {
            Id = 1,
            Model = SubdeviceModel.AC1100,
            GwIp = "1.2.3.4"
        };
        subdevice.Sensors.Add(new Sensor("water_total", "Total Water", 1000.0, SensorDataType.Double, "L", SensorType.Water));
        subdevice.Sensors.Add(new Sensor("happen_water", "Last Planned Consumption", 800.0, SensorDataType.Double, "L", SensorType.Water));

        SensorBuilder.CalculateWFC01Addons(ref subdevice);

        // should not modify since model is AC1100
        var happen = subdevice.Sensors.First(s => s.Name == "happen_water");
        Assert.That((double)happen.Value!, Is.EqualTo(800.0).Within(0.01));
    }
}
