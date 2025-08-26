using Ecowitt.Controller.Model;

namespace Ecowitt.Controller.Service.Orchestrator
{
    public static class DeNoiserHelper
    {
        private static readonly Dictionary<SensorType, double> ToleranceMap = new()
        {
            { SensorType.Temperature, 0.1 },
            { SensorType.Humidity, 1.0 },
            { SensorType.Pressure, 0.5 },
            { SensorType.Battery, 1.0 }
        };

        public static bool HasSignificantChange<T>(ISensor<T> sensor, T newValue)
        {
            if (sensor.Value == null || newValue == null)
                return !Equals(sensor.Value, newValue);

            if (typeof(T) == typeof(double) || typeof(T) == typeof(float))
            {
                var tolerance = ToleranceMap.GetValueOrDefault(sensor.SensorType, 0.01);
                var oldVal = Convert.ToDouble(sensor.Value);
                var newVal = Convert.ToDouble(newValue);
                return Math.Abs(oldVal - newVal) >= tolerance;
            }

            return !Equals(sensor.Value, newValue);
        }
        public static bool HasSignificantChange(ISensor sensor, object newValue)
        {
            if (sensor.Value == null || newValue == null)
                return !Equals(sensor.Value, newValue);

            // Handle numeric types with tolerance
            if (sensor.DataType == typeof(double) || sensor.DataType == typeof(float))
            {
                var tolerance = ToleranceMap.GetValueOrDefault(sensor.SensorType, 0.01);
                var oldVal = Convert.ToDouble(sensor.Value);
                var newVal = Convert.ToDouble(newValue);
                return Math.Abs(oldVal - newVal) >= tolerance;
            }

            // Handle integer types with tolerance
            if (sensor.DataType == typeof(int) || sensor.DataType == typeof(long))
            {
                var tolerance = ToleranceMap.GetValueOrDefault(sensor.SensorType, 1.0);
                var oldVal = Convert.ToDouble(sensor.Value);
                var newVal = Convert.ToDouble(newValue);
                return Math.Abs(oldVal - newVal) >= tolerance;
            }

            return !Equals(sensor.Value, newValue);
        }
    }
}
