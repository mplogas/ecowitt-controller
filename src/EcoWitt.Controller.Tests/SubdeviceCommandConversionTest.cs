using Ecowitt.Controller.Model.Api;
using Ecowitt.Controller.Service.Http;
using NUnit.Framework;

namespace EcoWitt.Controller.Tests;

[TestFixture]
public class SubdeviceCommandConversionTest
{
    [Test]
    public void Convert_Minutes_ToSecondsValType0()
    {
        var (valType, val) = HttpPublishingService.ToGatewayRun(10, DurationUnit.Minutes);
        Assert.That(valType, Is.EqualTo(0));
        Assert.That(val, Is.EqualTo(600));   // 10 min = 600 s
    }

    [Test]
    public void Convert_Seconds_PassThroughValType0()
    {
        var (valType, val) = HttpPublishingService.ToGatewayRun(180, DurationUnit.Seconds);
        Assert.That(valType, Is.EqualTo(0));
        Assert.That(val, Is.EqualTo(180));
    }

    [Test]
    public void Convert_Hours_ToSecondsValType0()
    {
        var (valType, val) = HttpPublishingService.ToGatewayRun(1, DurationUnit.Hours);
        Assert.That(valType, Is.EqualTo(0));
        Assert.That(val, Is.EqualTo(3600));
    }

    [Test]
    public void Convert_Liters_ToDeciliterValType3()
    {
        var (valType, val) = HttpPublishingService.ToGatewayRun(5, DurationUnit.Liters);
        Assert.That(valType, Is.EqualTo(3));
        Assert.That(val, Is.EqualTo(50));    // 5 L = 50 dL
    }
}
