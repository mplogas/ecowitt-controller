using Ecowitt.Controller.Model;

namespace Ecowitt.Controller.Model.Api;

public enum RunModeKey
{
    Duration = 0,
    Volume = 1
}

public record RunMode(
    RunModeKey Key,
    string HaLabel,
    DurationUnit Unit,
    int Min,
    int Max,
    int Default);

public static class RunModeRegistry
{
    // Single source of truth for run-mode bounds/defaults, consumed by HA discovery
    // (number min/max/initial) and the Dispatcher guard (clamp/default).
    private static readonly RunMode Duration =
        new(RunModeKey.Duration, "Duration", DurationUnit.Minutes, 1, 1440, 3);
    private static readonly RunMode Volume =
        new(RunModeKey.Volume, "Volume", DurationUnit.Liters, 1, 1000, 5);

    public static RunMode ByKey(RunModeKey key) => key switch
    {
        RunModeKey.Duration => Duration,
        RunModeKey.Volume => Volume,
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "unknown run mode")
    };

    public static bool SupportsDuration(SubdeviceModel model) =>
        model is SubdeviceModel.WFC01 or SubdeviceModel.WFC02 or SubdeviceModel.AC1100;

    public static bool SupportsVolume(SubdeviceModel model, bool hasFlowMeter) => model switch
    {
        SubdeviceModel.WFC01 => true,            // flow built in; not self-reported
        SubdeviceModel.WFC02 => hasFlowMeter,    // optional flow, self-reported
        _ => false                                // AC1100 and others: no flow
    };

    // Modes applicable to a device, in display order.
    public static IReadOnlyList<RunMode> ApplicableModes(SubdeviceModel model, bool hasFlowMeter)
    {
        var modes = new List<RunMode>();
        if (SupportsDuration(model)) modes.Add(Duration);
        if (SupportsVolume(model, hasFlowMeter)) modes.Add(Volume);
        return modes;
    }
}
