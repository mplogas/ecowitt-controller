using Ecowitt.Controller.Model;

namespace Ecowitt.Controller.Mapping
{
    public partial class SensorBuilder
    {
        public static void CalculateWFC01Addons(ref Model.Subdevice subdevice)
        {
            if (subdevice.Model == SubdeviceModel.WFC01)
            {
                var waterTotal = subdevice.Sensors.FirstOrDefault(s =>
                    s.Name.Equals("water_total", StringComparison.InvariantCultureIgnoreCase));
                var waterHappen = subdevice.Sensors.FirstOrDefault(s =>
                    s.Name.Equals("happen_water", StringComparison.InvariantCultureIgnoreCase));

                if (waterTotal != null && waterHappen != null)
                {
                    waterHappen.Value = (double)waterTotal.Value - (double)waterHappen.Value;
                }
            }
        }
    }
}
