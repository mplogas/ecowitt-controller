using Ecowitt.Controller.Model.Api;
using Ecowitt.Controller.Service.Orchestrator;
using NUnit.Framework;

namespace EcoWitt.Controller.Tests;

[TestFixture]
public class DispatcherGuardTest
{
    [Test]
    public void Guard_DurationUnset_DefaultsTo3Min()
    {
        var cmd = new SubdeviceApiCommand { Cmd = Command.Start, Id = 1, Unit = DurationUnit.Minutes, Duration = null };
        Dispatcher.GuardRunCommand(cmd);
        Assert.That(cmd.Duration, Is.EqualTo(3));
        Assert.That(cmd.AlwaysOn, Is.Not.EqualTo(true));
    }

    [Test]
    public void Guard_NegativeDuration_DefaultsTo3Min()
    {
        var cmd = new SubdeviceApiCommand { Cmd = Command.Start, Id = 1, Unit = DurationUnit.Minutes, Duration = -1 };
        Dispatcher.GuardRunCommand(cmd);
        Assert.That(cmd.Duration, Is.EqualTo(3));
    }

    [Test]
    public void Guard_HugeDuration_ClampedToMax()
    {
        var cmd = new SubdeviceApiCommand { Cmd = Command.Start, Id = 1, Unit = DurationUnit.Minutes, Duration = int.MaxValue };
        Dispatcher.GuardRunCommand(cmd);
        Assert.That(cmd.Duration, Is.EqualTo(1440));
    }

    [Test]
    public void Guard_VolumeUnset_DefaultsTo5L()
    {
        var cmd = new SubdeviceApiCommand { Cmd = Command.Start, Id = 1, Unit = DurationUnit.Liters, Duration = 0 };
        Dispatcher.GuardRunCommand(cmd);
        Assert.That(cmd.Duration, Is.EqualTo(5));
    }

    [Test]
    public void Guard_BareStartNoUnit_StaysAlwaysOn()
    {
        // The HA switch ON: Start, no Unit, no Duration → always-on, NOT defaulted to a duration run.
        var cmd = new SubdeviceApiCommand { Cmd = Command.Start, Id = 1, Unit = null, Duration = null };
        Dispatcher.GuardRunCommand(cmd);
        Assert.That(cmd.AlwaysOn, Is.True);
        Assert.That(cmd.Duration, Is.Null);
    }

    [Test]
    public void Guard_Stop_Untouched()
    {
        var cmd = new SubdeviceApiCommand { Cmd = Command.Stop, Id = 1 };
        Dispatcher.GuardRunCommand(cmd);
        Assert.That(cmd.AlwaysOn, Is.Not.EqualTo(true));
        Assert.That(cmd.Duration, Is.Null);
    }

}
