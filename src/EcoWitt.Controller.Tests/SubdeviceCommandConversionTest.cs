using Ecowitt.Controller.Model.Api;
using Ecowitt.Controller.Service.Http;
using NUnit.Framework;

namespace EcoWitt.Controller.Tests;

[TestFixture]
public class SubdeviceCommandConversionTest
{
    // Ground truth: val_type is the gateway's unit selector and val is the value IN that unit
    // (docs/api.md: 0=Seconds, 1=Minutes, 2=Hours, 3=Liters). Confirmed on a WFC01 (id 14391):
    // 10 s -> {val_type:0, val:10}, 10 min -> {val_type:1, val:10}. The controller must NOT
    // normalize time to seconds; the gateway caps/rejects large second counts and falls back to a
    // ~3-minute default run, which made every multi-minute duration collapse to 3 minutes.

    [Test]
    public void Convert_Minutes_ToNativeValType1()
    {
        var (valType, val) = HttpPublishingService.ToGatewayRun(10, DurationUnit.Minutes);
        Assert.That(valType, Is.EqualTo(1)); // 1 = Minutes
        Assert.That(val, Is.EqualTo(10));    // value passed through, not converted to seconds
    }

    [Test]
    public void Convert_Seconds_PassThroughValType0()
    {
        var (valType, val) = HttpPublishingService.ToGatewayRun(10, DurationUnit.Seconds);
        Assert.That(valType, Is.EqualTo(0)); // 0 = Seconds
        Assert.That(val, Is.EqualTo(10));
    }

    [Test]
    public void Convert_Hours_ToMinutesValType1()
    {
        // val_type:2 (Hours) is documented but unverified and not exposed in the UI,
        // so hours are sent using the confirmed minutes encoding instead.
        var (valType, val) = HttpPublishingService.ToGatewayRun(1, DurationUnit.Hours);
        Assert.That(valType, Is.EqualTo(1)); // 1 = Minutes (confirmed)
        Assert.That(val, Is.EqualTo(60));    // 1 h = 60 min
    }

    [Test]
    public void Convert_Liters_ToDeciliterValType3()
    {
        var (valType, val) = HttpPublishingService.ToGatewayRun(5, DurationUnit.Liters);
        Assert.That(valType, Is.EqualTo(3));
        Assert.That(val, Is.EqualTo(50));    // 5 L = 50 dL (liters x 10), verified on a WFC02
    }
}
