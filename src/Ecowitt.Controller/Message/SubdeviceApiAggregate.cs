using Ecowitt.Controller.Model.Api;

namespace Ecowitt.Controller.Message;

public class SubdeviceApiAggregate
{
    public List<SubdeviceApiData> Subdevices { get; set; } = new();
}