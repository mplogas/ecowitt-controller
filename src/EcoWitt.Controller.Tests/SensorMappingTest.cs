using Ecowitt.Controller.Model;
using Newtonsoft.Json;

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
        // arrange act assert
        List<ISensor> list = new();
        
        Sensor<bool> sensorBool = new("Test", true, "", SensorType.None, SensorState.Measurement);
        Assert.That(sensorBool.DataType, Is.EqualTo(typeof(bool)));
        Assert.That(sensorBool.Value, Is.EqualTo(true));
        Assert.That(sensorBool.Name, Is.EqualTo("Test"));
        
        Sensor<int> sensorInt = new("Test", 1, "", SensorType.None, SensorState.Measurement);
        Assert.That(sensorInt.DataType, Is.EqualTo(typeof(int)));
        Assert.That(sensorInt.Value, Is.EqualTo(1));
        Assert.That(sensorInt.Name, Is.EqualTo("Test"));
        
        Sensor<double> sensorDouble = new("Test", 1.0, "", SensorType.Temperature, SensorState.Measurement);
        Assert.That(sensorDouble.DataType, Is.EqualTo(typeof(double)));
        Assert.That(sensorDouble.Value, Is.EqualTo(1.0));
        Assert.That(sensorDouble.SensorType, Is.EqualTo(SensorType.Temperature));
        Assert.That(sensorDouble.Name, Is.EqualTo("Test"));
        
        Sensor<string> sensorString = new("Test", "10", "GBps", SensorType.DataRate, SensorState.Measurement);
        Assert.That(sensorString.DataType, Is.EqualTo(typeof(string)));
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