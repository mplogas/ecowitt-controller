using Ecowitt.Controller.Model;

namespace EcoWitt.Controller.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestSensorCreation()
    {
        List<ISensor> list = new();
        
        var sensorBool = new Sensor("Test", true, SensorDataType.Boolean, sensorType: SensorType.None, sensorState: SensorState.Measurement);
        Assert.That(sensorBool.DataType, Is.EqualTo(SensorDataType.Boolean));
        Assert.That(sensorBool.Value, Is.EqualTo(true));
        Assert.That(sensorBool.Name, Is.EqualTo("Test"));
        
        var sensorInt = new Sensor("Test", 1, SensorDataType.Integer, sensorType: SensorType.None, sensorState: SensorState.Measurement);
        Assert.That(sensorInt.DataType, Is.EqualTo(SensorDataType.Integer));
        Assert.That(sensorInt.Value, Is.EqualTo(1));
        Assert.That(sensorInt.Name, Is.EqualTo("Test"));
        
        var sensorDouble = new Sensor("Test", 1.0, SensorDataType.Double, sensorType: SensorType.Temperature, sensorState: SensorState.Measurement);
        Assert.That(sensorDouble.DataType, Is.EqualTo(SensorDataType.Double));
        Assert.That(sensorDouble.Value, Is.EqualTo(1.0));
        Assert.That(sensorDouble.SensorType, Is.EqualTo(SensorType.Temperature));
        Assert.That(sensorDouble.Name, Is.EqualTo("Test"));
        
        var sensorString = new Sensor("Test", "10", SensorDataType.String, unitOfMeasurement: "GBps", sensorType: SensorType.DataRate, sensorState: SensorState.Measurement);
        Assert.That(sensorString.DataType, Is.EqualTo(SensorDataType.String));
        Assert.That(sensorString.Value, Is.EqualTo("10"));
        Assert.That(sensorString.UnitOfMeasurement, Is.EqualTo("GBps"));
        Assert.That(sensorString.Name, Is.EqualTo("Test"));
        
        list.Add(sensorBool);
        list.Add(sensorInt);
        list.Add(sensorDouble);
        list.Add(sensorString);
        Assert.That(list.Count, Is.EqualTo(4));
    }
}