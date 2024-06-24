using Ecowitt.Controller.Configuration;
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
    private readonly EcowittOptions _options;

    public DataConsumer(ILogger<DataConsumer> logger, IDeviceStore deviceStore, EcowittOptions options)
    {
        _logger = logger;
        _deviceStore = deviceStore;
        _options = options;
    }

    public async Task OnHandle(ApiData message)
    {
        _logger.LogInformation($"Received ApiData: {message.PASSKEY}");
        var gw = message.Map();
        if(!_deviceStore.UpsertGateway(gw)) _logger.LogWarning($"failed to store {gw.IpAddress} ({gw.Model}) update");
    }

    public Task OnHandle(SubdeviceData message)
    {
        //optimization: get all subdevices per gateway at once
        // then iterate over the list of subdevices and update the gateway
        // rinse and repeat for all subdevies groups

        var ips = message.Subdevices.DistinctBy(sd => sd.GwIp).Select(sd => sd.GwIp);

        message.Subdevices.ForEach(d =>
        {
            var gw = _deviceStore.GetGateway(d.GwIp);
            if (gw == null)
            {
                if (_options.AutoDiscovery)
                {
                    _logger.LogWarning($"Gateway {d.GwIp} not found. Not updating subdevice {d.Id} ({d.Model})");
                    return;
                }
                else
                {
                    gw = new Gateway {IpAddress = d.GwIp};
                    _logger.LogWarning($"Gateway {d.GwIp} not found, auto-discovery disabled. Creating new empty gateway");
                }
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
            
            _deviceStore.UpsertGateway(gw);
        });
        
        return Task.CompletedTask;
    }
}