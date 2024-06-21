using Ecowitt.Controller.Model;
using Ecowitt.Controller.Mapping;
using Ecowitt.Controller.Mqtt;
using Ecowitt.Controller.Store;
using SlimMessageBus;

namespace Ecowitt.Controller.Consumer;

public class DataConsumer : IConsumer<ApiData>, IConsumer<SubdeviceData>
{
    private readonly ILogger<DataConsumer> _logger;
    private readonly IDeviceStore _deviceStore;

    public DataConsumer(ILogger<DataConsumer> logger, IDeviceStore deviceStore)
    {
        _logger = logger;
        _deviceStore = deviceStore;
    }

    public async Task OnHandle(ApiData message)
    {
        _logger.LogInformation($"Received ApiData: {message.PASSKEY}");
        var gw = message.Map();
        if(!_deviceStore.UpsertGateway(gw)) _logger.LogWarning($"failed to store {gw.IpAddress} ({gw.Model}) update");
    }

    public Task OnHandle(SubdeviceData message)
    {
        message.Subdevices.ForEach(d =>
        {
            var gw =_deviceStore.GetGateway(d.GwIp);
            if (gw == null)
            {
                _logger.LogWarning($"Gateway {d.GwIp} not found. Not updating subdevice {d.Id} ({d.Model})");
                return;
            }
            
            var subdevice = gw.Subdevices.FirstOrDefault(sd => sd.Id == d.Id);
            if (subdevice == null)
            {
                gw.Subdevices.Add(d);
                _logger.LogInformation($"subdevice added: {d.Id} ({d.Model})");
            }
            else
            {
                subdevice.Battery = d.Battery;
                subdevice.RfnetState = d.RfnetState;
                subdevice.Signal = d.Signal;
                subdevice.Ver = d.Ver;
                subdevice.Payload = d.Payload;
                
                _logger.LogInformation($"subdevice updated: {d.Id} ({d.Model})");
            }
        });
        
        return Task.CompletedTask;
    }
}