using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Api;
using Ecowitt.Controller.Model.Configuration;
using Ecowitt.Controller.Model.Message.Data;
using Ecowitt.Controller.Service.Orchestrator;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using SlimMessageBus;

namespace EcoWitt.Controller.Tests;

/// <summary>
/// Tests for Dispatcher.OnHandle(SubdeviceRunConfig) — staged config merge,
/// assembly into SubdeviceApiCommand, and edge-case guard paths.
/// Spec reference: docs/specs/2026-06-03-valve-run-modes-design.md lines 332–335.
/// </summary>
[TestFixture]
public class DispatcherMergeTest
{
    // ------------------------------------------------------------------
    // Fakes
    // ------------------------------------------------------------------

    private sealed class FakeDeviceStore : IDeviceStore
    {
        private readonly Dictionary<string, Device> _store = new();

        public void Seed(Device device) => _store[device.IpAddress] = device;

        public Device? GetGateway(string ipAddress)
            => _store.TryGetValue(ipAddress, out var gw) ? gw : null;

        public Device? GetGatewayBySubdeviceId(int id)
            => _store.Values.FirstOrDefault(g => g.Subdevices.Any(s => s.Id == id));

        public bool UpsertGateway(Device data)
        {
            _store[data.IpAddress] = data;
            return true;
        }

        public void Clear() => _store.Clear();

        public Dictionary<string, string?> GetGatewaysShort()
            => _store.ToDictionary(kv => kv.Key, kv => kv.Value.Model);
    }

    private sealed class CapturingMessageBus : IMessageBus
    {
        public readonly List<object> Published = new();

        public Task Publish<TMessage>(TMessage message, string? path = null,
            IDictionary<string, object>? headers = null,
            CancellationToken cancellationToken = default)
        {
            Published.Add(message!);
            return Task.CompletedTask;
        }

        // Request/response stubs — not used by Dispatcher but required by the interface.
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, string? path = null,
            IDictionary<string, object>? headers = null, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Send(IRequest request, string? path = null,
            IDictionary<string, object>? headers = null, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<TResponse> Send<TResponse, TRequest>(TRequest request, string? path = null,
            IDictionary<string, object>? headers = null, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static Dispatcher BuildDispatcher(FakeDeviceStore store, CapturingMessageBus bus)
    {
        var logger = NullLogger<Dispatcher>.Instance;
        return new Dispatcher(
            logger,
            store,
            bus,
            Options.Create(new MqttOptions()),
            Options.Create(new EcowittOptions()),
            Options.Create(new ControllerOptions()));
    }

    private static (FakeDeviceStore store, CapturingMessageBus bus, Dispatcher dispatcher, Subdevice subdevice)
        SetupWfc02()
    {
        var store = new FakeDeviceStore();
        var bus = new CapturingMessageBus();
        var dispatcher = BuildDispatcher(store, bus);

        var subdevice = new Subdevice
        {
            Id = 99,
            Model = SubdeviceModel.WFC02,
            HasFlowMeter = true,
            GwIp = "192.168.1.10",
            Nickname = "WFC02-test",
            Devicename = "WFC02",
            StagedRunConfig = new StagedRunConfig() // defaults: Mode=Duration, DurationMinutes=3, VolumeLiters=5
        };

        var gateway = new Device
        {
            IpAddress = "192.168.1.10",
            Name = "test-gateway",
            Subdevices = new List<Subdevice> { subdevice }
        };

        store.Seed(gateway);
        return (store, bus, dispatcher, subdevice);
    }

    // ------------------------------------------------------------------
    // Tests
    // ------------------------------------------------------------------

    [Test]
    public async Task Merge_PartialUpdates_ApplyToStagedConfig()
    {
        var (_, _, dispatcher, subdevice) = SetupWfc02();

        // Set mode to Volume
        await dispatcher.OnHandle(
            new SubdeviceRunConfig { Id = 99, Mode = RunModeKey.Volume, Start = false },
            CancellationToken.None);

        Assert.That(subdevice.StagedRunConfig.Mode, Is.EqualTo(RunModeKey.Volume));

        // Set volume to 10 L
        await dispatcher.OnHandle(
            new SubdeviceRunConfig { Id = 99, Volume = 10, Start = false },
            CancellationToken.None);

        Assert.That(subdevice.StagedRunConfig.VolumeLiters, Is.EqualTo(10));
    }

    [Test]
    public async Task Start_DurationMode_EmitsCorrectCommand()
    {
        var (_, bus, dispatcher, subdevice) = SetupWfc02();
        subdevice.StagedRunConfig.Mode = RunModeKey.Duration;
        subdevice.StagedRunConfig.DurationMinutes = 7;

        await dispatcher.OnHandle(
            new SubdeviceRunConfig { Id = 99, Start = true },
            CancellationToken.None);

        var dispatch = bus.Published.OfType<SubdeviceCommandDispatch>().Single();
        Assert.That(dispatch.Command.Cmd, Is.EqualTo(Command.Start));
        Assert.That(dispatch.Command.Unit, Is.EqualTo(DurationUnit.Minutes));
        Assert.That(dispatch.Command.Duration, Is.EqualTo(7));
        Assert.That(dispatch.GatewayIp, Is.EqualTo("192.168.1.10"));
        Assert.That(dispatch.Model, Is.EqualTo(SubdeviceModel.WFC02));
    }

    [Test]
    public async Task Start_VolumeMode_EmitsCorrectCommand()
    {
        var (_, bus, dispatcher, subdevice) = SetupWfc02();
        subdevice.StagedRunConfig.Mode = RunModeKey.Volume;
        subdevice.StagedRunConfig.VolumeLiters = 15;

        await dispatcher.OnHandle(
            new SubdeviceRunConfig { Id = 99, Start = true },
            CancellationToken.None);

        var dispatch = bus.Published.OfType<SubdeviceCommandDispatch>().Single();
        Assert.That(dispatch.Command.Cmd, Is.EqualTo(Command.Start));
        Assert.That(dispatch.Command.Unit, Is.EqualTo(DurationUnit.Liters));
        Assert.That(dispatch.Command.Duration, Is.EqualTo(15));
    }

    [Test]
    public async Task Start_MergedVolumeMode_ThenFire_EmitsAssembledCommand()
    {
        // Spec: feed SubdeviceRunConfig partials (mode, then volume), then Start=true
        // → Dispatcher emits the correct SubdeviceApiCommand from the staged config.
        var (_, bus, dispatcher, _) = SetupWfc02();

        await dispatcher.OnHandle(
            new SubdeviceRunConfig { Id = 99, Mode = RunModeKey.Volume, Start = false },
            CancellationToken.None);

        await dispatcher.OnHandle(
            new SubdeviceRunConfig { Id = 99, Volume = 20, Start = false },
            CancellationToken.None);

        await dispatcher.OnHandle(
            new SubdeviceRunConfig { Id = 99, Start = true },
            CancellationToken.None);

        var dispatches = bus.Published.OfType<SubdeviceCommandDispatch>().ToList();
        Assert.That(dispatches, Has.Count.EqualTo(1));
        var cmd = dispatches[0].Command;
        Assert.That(cmd.Unit, Is.EqualTo(DurationUnit.Liters));
        Assert.That(cmd.Duration, Is.EqualTo(20));
    }

    [Test]
    public async Task Start_ZeroDurationInStagedConfig_WarnsAndDoesNotDispatch()
    {
        // Spec: selected-mode input of 0 → warning, no dispatch.
        // When staged DurationMinutes = 0, Start must be suppressed (not silently defaulted).
        var (_, bus, dispatcher, subdevice) = SetupWfc02();
        subdevice.StagedRunConfig.Mode = RunModeKey.Duration;
        subdevice.StagedRunConfig.DurationMinutes = 0; // explicitly bad value

        await dispatcher.OnHandle(
            new SubdeviceRunConfig { Id = 99, Start = true },
            CancellationToken.None);

        var dispatches = bus.Published.OfType<SubdeviceCommandDispatch>().ToList();
        Assert.That(dispatches, Is.Empty, "Zero duration must not be dispatched");
    }

    [Test]
    public async Task Start_ZeroVolumeInStagedConfig_WarnsAndDoesNotDispatch()
    {
        // Spec: selected-mode input of 0 → warning, no dispatch (volume variant).
        var (_, bus, dispatcher, subdevice) = SetupWfc02();
        subdevice.StagedRunConfig.Mode = RunModeKey.Volume;
        subdevice.StagedRunConfig.VolumeLiters = 0; // explicitly bad value

        await dispatcher.OnHandle(
            new SubdeviceRunConfig { Id = 99, Start = true },
            CancellationToken.None);

        var dispatches = bus.Published.OfType<SubdeviceCommandDispatch>().ToList();
        Assert.That(dispatches, Is.Empty, "Zero volume must not be dispatched");
    }

    [Test]
    public async Task UnknownSubdevice_LogsWarningAndDoesNotDispatch()
    {
        var (_, bus, dispatcher, _) = SetupWfc02();

        await dispatcher.OnHandle(
            new SubdeviceRunConfig { Id = 999, Start = true },
            CancellationToken.None);

        Assert.That(bus.Published.OfType<SubdeviceCommandDispatch>(), Is.Empty);
    }

    [Test]
    public async Task NoStart_DoesNotDispatch()
    {
        var (_, bus, dispatcher, _) = SetupWfc02();

        await dispatcher.OnHandle(
            new SubdeviceRunConfig { Id = 99, Duration = 10, Start = false },
            CancellationToken.None);

        Assert.That(bus.Published.OfType<SubdeviceCommandDispatch>(), Is.Empty);
    }
}
