using Ecowitt.Controller.Model;
using Serilog;

namespace Ecowitt.Controller.Service.Orchestrator
{
    public static class DeNoiserHelper
    {
        // Base tolerances per SensorType (can be extended)
        private static readonly Dictionary<SensorType, double> TypeTolerance = new()
        {
            { SensorType.Temperature, 0.1 },
            { SensorType.Humidity, 1.0 },
            { SensorType.Pressure, 0.5 },
            { SensorType.Battery, 1.0 },
            { SensorType.WindSpeed, 0.2 },
            { SensorType.PrecipitationIntensity, 0.1 },
            { SensorType.Distance, 1.0 }
        };

        private const double DefaultDoubleTolerance = 0.01;
        private const double DefaultIntegerTolerance = 1.0;

        public static bool HasSignificantChange(ISensor sensor, object? newValue)
        {
            if (sensor.Value == null && newValue == null) return false;
            if (sensor.Value == null || newValue == null) return true; // one is null => changed

            try
            {
                switch (sensor.DataType)
                {
                    case SensorDataType.Double:
                    {
                        var oldVal = Convert.ToDouble(sensor.Value);
                        var newVal = Convert.ToDouble(newValue);
                        var tol = GetTolerance(sensor.SensorType, DefaultDoubleTolerance);
                        var changed = Math.Abs(oldVal - newVal) >= tol;
                        Log.Verbose("DeNoiser double check {Name}: old={Old} new={New} tol={Tol} changed={Changed}", sensor.Name, oldVal, newVal, tol, changed);
                        return changed;
                    }
                    case SensorDataType.Integer:
                    {
                        var oldVal = Convert.ToDouble(sensor.Value); // cast to double for diff
                        var newVal = Convert.ToDouble(newValue);
                        var tol = GetTolerance(sensor.SensorType, DefaultIntegerTolerance);
                        var changed = Math.Abs(oldVal - newVal) >= tol;
                        Log.Verbose("DeNoiser int check {Name}: old={Old} new={New} tol={Tol} changed={Changed}", sensor.Name, oldVal, newVal, tol, changed);
                        return changed;
                    }
                    case SensorDataType.Boolean:
                    case SensorDataType.String:
                    case SensorDataType.DateTime:
                        var eq = Equals(sensor.Value, newValue);
                        Log.Verbose("DeNoiser equality check {Name}: old={Old} new={New} changed={Changed}", sensor.Name, sensor.Value, newValue, !eq);
                        return !eq;
                    default:
                        return !Equals(sensor.Value, newValue);
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "DeNoiser parse error for sensor {Name} (treating as changed)", sensor.Name);
                return true;
            }
        }

        private static double GetTolerance(SensorType type, double fallback)
            => TypeTolerance.TryGetValue(type, out var t) ? t : fallback;
    }
}
