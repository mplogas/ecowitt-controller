using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Api;
using NUnit.Framework;

namespace EcoWitt.Controller.Tests;

[TestFixture]
public class RunModeTest
{
    [Test]
    public void Registry_Duration_HasMinutesUnitAndDefaults()
    {
        var m = RunModeRegistry.ByKey(RunModeKey.Duration);
        Assert.That(m.Unit, Is.EqualTo(DurationUnit.Minutes));
        Assert.That(m.Min, Is.EqualTo(1));
        Assert.That(m.Max, Is.EqualTo(1440));
        Assert.That(m.Default, Is.EqualTo(3));
    }

    [Test]
    public void Registry_Volume_HasLitersUnitAndDefaults()
    {
        var m = RunModeRegistry.ByKey(RunModeKey.Volume);
        Assert.That(m.Unit, Is.EqualTo(DurationUnit.Liters));
        Assert.That(m.Min, Is.EqualTo(1));
        Assert.That(m.Max, Is.EqualTo(1000));
        Assert.That(m.Default, Is.EqualTo(5));
    }

    [Test]
    public void Capability_AC1100_DurationOnly()
    {
        Assert.That(RunModeRegistry.SupportsDuration(SubdeviceModel.AC1100), Is.True);
        Assert.That(RunModeRegistry.SupportsVolume(SubdeviceModel.AC1100, hasFlowMeter: false), Is.False);
    }

    [Test]
    public void Capability_WFC01_DurationAndVolume()
    {
        Assert.That(RunModeRegistry.SupportsDuration(SubdeviceModel.WFC01), Is.True);
        // WFC01 always has flow regardless of the reported flag
        Assert.That(RunModeRegistry.SupportsVolume(SubdeviceModel.WFC01, hasFlowMeter: false), Is.True);
    }

    [Test]
    public void Capability_WFC02_VolumeGatedByFlowMeter()
    {
        Assert.That(RunModeRegistry.SupportsVolume(SubdeviceModel.WFC02, hasFlowMeter: true), Is.True);
        Assert.That(RunModeRegistry.SupportsVolume(SubdeviceModel.WFC02, hasFlowMeter: false), Is.False);
    }

    [Test]
    public void ApplicableModes_WFC02WithFlow_DurationThenVolume()
    {
        var modes = RunModeRegistry.ApplicableModes(SubdeviceModel.WFC02, hasFlowMeter: true);
        Assert.That(modes.Select(m => m.Key), Is.EqualTo(new[] { RunModeKey.Duration, RunModeKey.Volume }));
    }

    [Test]
    public void ApplicableModes_AC1100_DurationOnly()
    {
        var modes = RunModeRegistry.ApplicableModes(SubdeviceModel.AC1100, hasFlowMeter: false);
        Assert.That(modes.Select(m => m.Key), Is.EqualTo(new[] { RunModeKey.Duration }));
    }

    [Test]
    public void IsApplicable_GatesVolumeByCapability()
    {
        // Duration is universal for controllable devices.
        Assert.That(RunModeRegistry.IsApplicable(SubdeviceModel.AC1100, hasFlowMeter: false, RunModeKey.Duration), Is.True);
        // Volume requires flow: AC1100 never; WFC02 only with flow; WFC01 always.
        Assert.That(RunModeRegistry.IsApplicable(SubdeviceModel.AC1100, hasFlowMeter: false, RunModeKey.Volume), Is.False);
        Assert.That(RunModeRegistry.IsApplicable(SubdeviceModel.WFC02, hasFlowMeter: false, RunModeKey.Volume), Is.False);
        Assert.That(RunModeRegistry.IsApplicable(SubdeviceModel.WFC02, hasFlowMeter: true, RunModeKey.Volume), Is.True);
        Assert.That(RunModeRegistry.IsApplicable(SubdeviceModel.WFC01, hasFlowMeter: false, RunModeKey.Volume), Is.True);
    }
}
