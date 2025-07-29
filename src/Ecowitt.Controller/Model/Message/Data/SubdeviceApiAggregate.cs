using Ecowitt.Controller.Model.Api;

namespace Ecowitt.Controller.Model.Message.Data;

public class SubdeviceApiAggregate
{
    public List<SubdeviceApiData> Subdevices { get; set; } = new List<SubdeviceApiData>();
}