using Ecowitt.Controller.Model.Api;

namespace Ecowitt.Controller.Model.Message.Data;

public class SubdeviceCommandDispatch
{
    public string GatewayIp { get; set; } = string.Empty;
    public SubdeviceApiCommand Command { get; set; } = new();
    public SubdeviceModel Model { get; set; } = SubdeviceModel.Unknown;
}
