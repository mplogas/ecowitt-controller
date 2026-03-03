using Ecowitt.Controller.Model;
using Ecowitt.Controller.Service.Orchestrator;

namespace EcoWitt.Controller.Tests;

public class DeNoiserTest
{
    [Test]
    public void Temperature_WithinTolerance_NoChange()
    {
        var sensor = new Sensor("tempf", 20.0, SensorDataType.Double, "°C", SensorType.Temperature);
        // tolerance is 0.1 for temperature
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, 20.05), Is.False);
    }

    [Test]
    public void Temperature_BeyondTolerance_Changed()
    {
        var sensor = new Sensor("tempf", 20.0, SensorDataType.Double, "°C", SensorType.Temperature);
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, 20.2), Is.True);
    }

    [Test]
    public void Humidity_WithinTolerance_NoChange()
    {
        var sensor = new Sensor("humidity", 50.0, SensorDataType.Double, "%", SensorType.Humidity);
        // tolerance is 1.0 for humidity
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, 50.5), Is.False);
    }

    [Test]
    public void Humidity_BeyondTolerance_Changed()
    {
        var sensor = new Sensor("humidity", 50.0, SensorDataType.Double, "%", SensorType.Humidity);
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, 51.5), Is.True);
    }

    [Test]
    public void Pressure_WithinTolerance_NoChange()
    {
        var sensor = new Sensor("baromrelin", 1013.0, SensorDataType.Double, "hPa", SensorType.Pressure);
        // tolerance is 0.5 for pressure
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, 1013.3), Is.False);
    }

    [Test]
    public void Battery_WithinTolerance_NoChange()
    {
        var sensor = new Sensor("batt1", 80.0, SensorDataType.Double, "%", SensorType.Battery);
        // tolerance is 1.0 for battery
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, 80.5), Is.False);
    }

    [Test]
    public void WindSpeed_BeyondTolerance_Changed()
    {
        var sensor = new Sensor("windspeedmph", 10.0, SensorDataType.Double, "km/h", SensorType.WindSpeed);
        // tolerance is 0.2 for wind speed
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, 10.3), Is.True);
    }

    [Test]
    public void Integer_WithinTolerance_NoChange()
    {
        var sensor = new Sensor("uv", 5, SensorDataType.Integer, sensorType: SensorType.None);
        // default integer tolerance is 1.0
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, 5), Is.False);
    }

    [Test]
    public void Integer_BeyondTolerance_Changed()
    {
        var sensor = new Sensor("uv", 5, SensorDataType.Integer, sensorType: SensorType.None);
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, 7), Is.True);
    }

    [Test]
    public void Boolean_Changed()
    {
        var sensor = new Sensor("ac_running", true, SensorDataType.Boolean, sensorType: SensorType.None, sensorClass: SensorClass.BinarySensor);
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, false), Is.True);
    }

    [Test]
    public void Boolean_Same_NoChange()
    {
        var sensor = new Sensor("ac_running", true, SensorDataType.Boolean, sensorType: SensorType.None, sensorClass: SensorClass.BinarySensor);
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, true), Is.False);
    }

    [Test]
    public void String_Changed()
    {
        var sensor = new Sensor("winddir-comp", "N", SensorDataType.String);
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, "NE"), Is.True);
    }

    [Test]
    public void String_Same_NoChange()
    {
        var sensor = new Sensor("winddir-comp", "N", SensorDataType.String);
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, "N"), Is.False);
    }

    [Test]
    public void BothNull_NoChange()
    {
        var sensor = new Sensor("test", null, SensorDataType.Double);
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, null), Is.False);
    }

    [Test]
    public void OldNull_NewValue_Changed()
    {
        var sensor = new Sensor("test", null, SensorDataType.Double);
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, 5.0), Is.True);
    }

    [Test]
    public void OldValue_NewNull_Changed()
    {
        var sensor = new Sensor("test", 5.0, SensorDataType.Double);
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, null), Is.True);
    }

    [Test]
    public void DefaultDoubleTolerance_UsedForUnmappedType()
    {
        // SensorType.None has no specific tolerance, falls back to 0.01
        var sensor = new Sensor("custom", 1.0, SensorDataType.Double, sensorType: SensorType.None);
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, 1.005), Is.False);
        Assert.That(DeNoiserHelper.HasSignificantChange(sensor, 1.02), Is.True);
    }
}
