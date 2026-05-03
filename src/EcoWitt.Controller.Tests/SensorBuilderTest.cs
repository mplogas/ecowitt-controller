using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Mapping;

namespace EcoWitt.Controller.Tests;

public class SensorBuilderTest
{
    [Test]
    public void BuildSensor_UnknownProperty_ReturnsNull()
    {
        var result = SensorBuilder.BuildSensor("unknown_property_xyz", "42");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void BuildSensor_TemperatureIndoor_MetricConversion()
    {
        var sensor = SensorBuilder.BuildSensor("tempinf", "68.0", isMetric: true);
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Name, Is.EqualTo("tempinf"));
        Assert.That(sensor.Alias, Is.EqualTo("Indoor Temperature"));
        Assert.That(sensor.SensorType, Is.EqualTo(SensorType.Temperature));
        Assert.That(sensor.UnitOfMeasurement, Is.EqualTo("°C"));
        Assert.That(sensor.DataType, Is.EqualTo(SensorDataType.Double));
        // 68°F = 20°C
        Assert.That((double)sensor.Value!, Is.EqualTo(20.0).Within(0.01));
    }

    [Test]
    public void BuildSensor_TemperatureIndoor_ImperialNoConversion()
    {
        var sensor = SensorBuilder.BuildSensor("tempinf", "68.0", isMetric: false);
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.UnitOfMeasurement, Is.EqualTo("F"));
        Assert.That((double)sensor.Value!, Is.EqualTo(68.0).Within(0.01));
    }

    [Test]
    public void BuildSensor_TemperatureOutdoor_MetricConversion()
    {
        var sensor = SensorBuilder.BuildSensor("tempf", "32.0", isMetric: true);
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Alias, Is.EqualTo("Outdoor Temperature"));
        // 32°F = 0°C
        Assert.That((double)sensor.Value!, Is.EqualTo(0.0).Within(0.01));
    }

    [Test]
    public void BuildSensor_NumberedTemperature()
    {
        var sensor = SensorBuilder.BuildSensor("tempf3", "50.0", isMetric: true);
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Alias, Is.EqualTo("Temperature 3"));
        // 50°F = 10°C
        Assert.That((double)sensor.Value!, Is.EqualTo(10.0).Within(0.01));
    }

    [Test]
    public void BuildSensor_Humidity()
    {
        var sensor = SensorBuilder.BuildSensor("humidity", "65.5");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Alias, Is.EqualTo("Outdoor Humidity"));
        Assert.That(sensor.SensorType, Is.EqualTo(SensorType.Humidity));
        Assert.That(sensor.UnitOfMeasurement, Is.EqualTo("%"));
        Assert.That((double)sensor.Value!, Is.EqualTo(65.5).Within(0.01));
    }

    [Test]
    public void BuildSensor_NumberedHumidity()
    {
        var sensor = SensorBuilder.BuildSensor("humidity5", "45.0");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Alias, Is.EqualTo("Humidity 5"));
    }

    [Test]
    public void BuildSensor_PressureRelative_MetricConversion()
    {
        var sensor = SensorBuilder.BuildSensor("baromrelin", "29.92", isMetric: true);
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Alias, Is.EqualTo("Relative Pressure"));
        Assert.That(sensor.SensorType, Is.EqualTo(SensorType.Pressure));
        Assert.That(sensor.UnitOfMeasurement, Is.EqualTo("hPa"));
        // 29.92 inHg * 33.86388 = ~1013.25 hPa
        Assert.That((double)sensor.Value!, Is.EqualTo(1013.25).Within(0.5));
    }

    [Test]
    public void BuildSensor_WindSpeed_MetricConversion()
    {
        var sensor = SensorBuilder.BuildSensor("windspeedmph", "10.0", isMetric: true);
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.SensorType, Is.EqualTo(SensorType.WindSpeed));
        Assert.That(sensor.UnitOfMeasurement, Is.EqualTo("km/h"));
        // 10 mph * 1.60934 = 16.0934 km/h
        Assert.That((double)sensor.Value!, Is.EqualTo(16.09).Within(0.01));
    }

    [Test]
    public void BuildSensor_WindGust()
    {
        var sensor = SensorBuilder.BuildSensor("windgustmph", "25.0", isMetric: true);
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Alias, Is.EqualTo("Wind Gust"));
    }

    [Test]
    public void BuildSensor_WindDirection()
    {
        var sensor = SensorBuilder.BuildSensor("winddir", "180");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.UnitOfMeasurement, Is.EqualTo("°"));
        Assert.That(sensor.Value, Is.EqualTo(180));
    }

    [Test]
    public void BuildSensor_SolarRadiation()
    {
        var sensor = SensorBuilder.BuildSensor("solarradiation", "850.5");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Alias, Is.EqualTo("Solar Radiation"));
        Assert.That(sensor.SensorType, Is.EqualTo(SensorType.Irradiance));
        Assert.That(sensor.UnitOfMeasurement, Is.EqualTo("W/m²"));
    }

    [Test]
    public void BuildSensor_UvIndex()
    {
        var sensor = SensorBuilder.BuildSensor("uv", "5");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Value, Is.EqualTo(5));
    }

    [Test]
    public void BuildSensor_RainRate_Metric()
    {
        var sensor = SensorBuilder.BuildSensor("rainratein", "0.5", isMetric: true);
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.SensorType, Is.EqualTo(SensorType.PrecipitationIntensity));
        Assert.That(sensor.UnitOfMeasurement, Is.EqualTo("mm/h"));
        // 0.5 in * 25.4 = 12.7 mm
        Assert.That((double)sensor.Value!, Is.EqualTo(12.7).Within(0.01));
    }

    [Test]
    public void BuildSensor_DailyRain_Metric()
    {
        var sensor = SensorBuilder.BuildSensor("dailyrainin", "1.0", isMetric: true);
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.SensorType, Is.EqualTo(SensorType.Precipitation));
        Assert.That(sensor.UnitOfMeasurement, Is.EqualTo("mm"));
        Assert.That((double)sensor.Value!, Is.EqualTo(25.4).Within(0.01));
    }

    [Test]
    public void BuildSensor_PiezoRainVariants()
    {
        Assert.That(SensorBuilder.BuildSensor("rrain_piezo", "0.1", true), Is.Not.Null);
        Assert.That(SensorBuilder.BuildSensor("erain_piezo", "0.2", true), Is.Not.Null);
        Assert.That(SensorBuilder.BuildSensor("hrain_piezo", "0.3", true), Is.Not.Null);
        Assert.That(SensorBuilder.BuildSensor("drain_piezo", "0.4", true), Is.Not.Null);
        Assert.That(SensorBuilder.BuildSensor("wrain_piezo", "0.5", true), Is.Not.Null);
        Assert.That(SensorBuilder.BuildSensor("mrain_piezo", "0.6", true), Is.Not.Null);
        Assert.That(SensorBuilder.BuildSensor("yrain_piezo", "0.7", true), Is.Not.Null);
    }

    [Test]
    public void BuildSensor_RainState_Binary()
    {
        var sensor = SensorBuilder.BuildSensor("srain_piezo", "1");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.SensorClass, Is.EqualTo(SensorClass.BinarySensor));
        Assert.That(sensor.Value, Is.EqualTo(true));
    }

    [Test]
    public void BuildSensor_SoilMoisture()
    {
        var sensor = SensorBuilder.BuildSensor("soilmoisture4", "55.0");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Alias, Is.EqualTo("Soil Moisture 4"));
        Assert.That(sensor.SensorType, Is.EqualTo(SensorType.Moisture));
    }

    [Test]
    public void BuildSensor_SoilAdmittance()
    {
        var sensor = SensorBuilder.BuildSensor("soilad2", "123");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Alias, Is.EqualTo("Soil Admittance 2"));
        Assert.That(sensor.UnitOfMeasurement, Is.Empty);
        Assert.That(sensor.SensorCategory, Is.EqualTo(SensorCategory.Diagnostic));
    }

    [Test]
    public void BuildSensor_SoilMoisture_Channel16()
    {
        var sensor = SensorBuilder.BuildSensor("soilmoisture16", "42.0");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Alias, Is.EqualTo("Soil Moisture 16"));
        Assert.That(sensor.SensorType, Is.EqualTo(SensorType.Moisture));
    }

    [Test]
    public void BuildSensor_SoilAdmittance_Channel16()
    {
        var sensor = SensorBuilder.BuildSensor("soilad16", "180");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Alias, Is.EqualTo("Soil Admittance 16"));
        Assert.That(sensor.SensorCategory, Is.EqualTo(SensorCategory.Diagnostic));
    }

    [Test]
    public void BuildSensor_SoilBattery_Channel16()
    {
        var sensor = SensorBuilder.BuildSensor("soilbatt16", "1.5");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Alias, Is.EqualTo("Soil Battery 16"));
        Assert.That(sensor.SensorType, Is.EqualTo(SensorType.Voltage));
    }

    [Test]
    public void BuildSensor_PM25Channel()
    {
        var sensor = SensorBuilder.BuildSensor("pm25_ch1", "12.5", isMetric: true);
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.SensorType, Is.EqualTo(SensorType.Pm25));
    }

    [Test]
    public void BuildSensor_CO2()
    {
        var sensor = SensorBuilder.BuildSensor("co2", "450");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.SensorType, Is.EqualTo(SensorType.CarbonDioxide));
        Assert.That(sensor.UnitOfMeasurement, Is.EqualTo("ppm"));
    }

    [Test]
    public void BuildSensor_Lightning()
    {
        var num = SensorBuilder.BuildSensor("lightning_num", "5");
        Assert.That(num, Is.Not.Null);
        Assert.That(num!.Alias, Is.EqualTo("Lightning Strikes"));

        var dist = SensorBuilder.BuildSensor("lightning", "10.5");
        Assert.That(dist, Is.Not.Null);
        Assert.That(dist!.SensorType, Is.EqualTo(SensorType.Distance));

        var time = SensorBuilder.BuildSensor("lightning_time", "1700000000");
        Assert.That(time, Is.Not.Null);
        Assert.That(time!.DataType, Is.EqualTo(SensorDataType.DateTime));
    }

    [Test]
    public void BuildSensor_Battery_WithMultiplier()
    {
        var sensor = SensorBuilder.BuildSensor("wh57batt", "3");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.SensorType, Is.EqualTo(SensorType.Battery));
        Assert.That(sensor.SensorCategory, Is.EqualTo(SensorCategory.Diagnostic));
        // 3 * 20 = 60%
        Assert.That(sensor.Value, Is.EqualTo(60));
    }

    [Test]
    public void BuildSensor_Battery_WithoutMultiplier()
    {
        var sensor = SensorBuilder.BuildSensor("batt1", "4");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor.Value, Is.EqualTo(4));
    }

    [Test]
    public void BuildSensor_Battery_ClampsTo100()
    {
        var sensor = SensorBuilder.BuildSensor("wh57batt", "6");
        Assert.That(sensor, Is.Not.Null);
        // 6 * 20 = 120 → clamped to 100
        Assert.That(sensor!.Value, Is.EqualTo(100));
    }

    [Test]
    public void BuildSensor_Voltage()
    {
        var sensor = SensorBuilder.BuildSensor("ws90cap_volt", "3.2");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.SensorType, Is.EqualTo(SensorType.Voltage));
        Assert.That(sensor.UnitOfMeasurement, Is.EqualTo("V"));
    }

    [Test]
    public void BuildSensor_ACSubdeviceSensors()
    {
        Assert.That(SensorBuilder.BuildSensor("ac_status", "1")!.SensorClass, Is.EqualTo(SensorClass.BinarySensor));
        Assert.That(SensorBuilder.BuildSensor("ac_running", "0")!.Value, Is.EqualTo(false));
        Assert.That(SensorBuilder.BuildSensor("realtime_power", "150")!.SensorType, Is.EqualTo(SensorType.Power));
        Assert.That(SensorBuilder.BuildSensor("ac_voltage", "230")!.SensorType, Is.EqualTo(SensorType.Voltage));
        Assert.That(SensorBuilder.BuildSensor("ac_current", "2")!.SensorType, Is.EqualTo(SensorType.Current));
        Assert.That(SensorBuilder.BuildSensor("elect_total", "5000")!.SensorType, Is.EqualTo(SensorType.Energy));
    }

    [Test]
    public void BuildSensor_WFCSubdeviceSensors()
    {
        Assert.That(SensorBuilder.BuildSensor("water_status", "1")!.SensorClass, Is.EqualTo(SensorClass.BinarySensor));
        Assert.That(SensorBuilder.BuildSensor("water_running", "0")!.Value, Is.EqualTo(false));

        var total = SensorBuilder.BuildSensor("water_total", "123.4", isMetric: true);
        Assert.That(total, Is.Not.Null);
        Assert.That(total!.SensorType, Is.EqualTo(SensorType.Water));

        var flow = SensorBuilder.BuildSensor("flow_velocity", "2.5", isMetric: true);
        Assert.That(flow, Is.Not.Null);
        Assert.That(flow!.SensorType, Is.EqualTo(SensorType.VolumeFlowRate));

        var temp = SensorBuilder.BuildSensor("water_temp", "65.3", isMetric: true);
        Assert.That(temp, Is.Not.Null);
        Assert.That(temp!.SensorType, Is.EqualTo(SensorType.Temperature));
    }

    [Test]
    public void BuildSensor_WFC02Sensors()
    {
        Assert.That(SensorBuilder.BuildSensor("wfc02_total", "500.0", true), Is.Not.Null);
        Assert.That(SensorBuilder.BuildSensor("wfc02_flow_velocity", "1.5", true), Is.Not.Null);
        Assert.That(SensorBuilder.BuildSensor("wfc02_status", "1"), Is.Not.Null);
        Assert.That(SensorBuilder.BuildSensor("wfc02rssi", "3"), Is.Not.Null);
        Assert.That(SensorBuilder.BuildSensor("wfc02batt", "80"), Is.Not.Null);
    }

    [Test]
    public void BuildSensor_LeafWetness()
    {
        var sensor = SensorBuilder.BuildSensor("leafwetness_ch3", "42");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Alias, Is.EqualTo("Leaf Wetness 3"));
    }

    [Test]
    public void BuildSensor_LeakChannel()
    {
        var sensor = SensorBuilder.BuildSensor("leak_ch2", "1");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.Alias, Is.EqualTo("Leak Channel 2"));
    }

    // Edge cases
    [TestCase("na")]
    [TestCase("--")]
    [TestCase("")]
    [TestCase("n/a")]
    [TestCase("nan")]
    [TestCase("null")]
    [TestCase("none")]
    public void BuildSensor_InvalidTokens_ReturnsNull(string value)
    {
        var sensor = SensorBuilder.BuildSensor("tempinf", value);
        Assert.That(sensor, Is.Null);
    }

    [Test]
    public void BuildSensor_CommaAsDecimal_ParsesCorrectly()
    {
        var sensor = SensorBuilder.BuildSensor("tempinf", "68,0", isMetric: true);
        Assert.That(sensor, Is.Not.Null);
        Assert.That((double)sensor!.Value!, Is.EqualTo(20.0).Within(0.01));
    }

    [Test]
    public void BuildSensor_DiagnosticCategory()
    {
        var sensor = SensorBuilder.BuildSensor("gw_rssi", "-64");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.SensorCategory, Is.EqualTo(SensorCategory.Diagnostic));
        Assert.That(sensor.SensorType, Is.EqualTo(SensorType.SignalStrength));
    }

    [Test]
    public void BuildSensor_HeapDiagnostic()
    {
        var sensor = SensorBuilder.BuildSensor("heap", "32000");
        Assert.That(sensor, Is.Not.Null);
        Assert.That(sensor!.SensorCategory, Is.EqualTo(SensorCategory.Diagnostic));
    }
}
