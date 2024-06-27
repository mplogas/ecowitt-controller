using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Mapping;
using Ecowitt.Controller.Store;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Ecowitt.Controller.Consumer;

public class DataConsumer : IConsumer<ApiData>, IConsumer<SubdeviceData>
{
    private readonly ILogger<DataConsumer> _logger;
    private readonly IDeviceStore _deviceStore;
    private readonly EcowittOptions _options;

    public DataConsumer(ILogger<DataConsumer> logger, IDeviceStore deviceStore, IOptions<EcowittOptions> options)
    {
        _logger = logger;
        _deviceStore = deviceStore;
        _options = options.Value;
    }

    public Task OnHandle(ApiData message)
    {
        _logger.LogInformation($"Received ApiData: {message.PASSKEY}");
        var gw = message.Map();
        gw.Name = _options.Gateways.FirstOrDefault(g => g.Ip == gw.IpAddress)?.Name ?? gw.IpAddress.Replace('.','-');
        if(!_deviceStore.UpsertGateway(gw)) _logger.LogWarning($"failed to store {gw.IpAddress} ({gw.Model}) update");
        
        return Task.CompletedTask;
    }

    public Task OnHandle(SubdeviceData message)
    {
        var ips = message.Subdevices.DistinctBy(sd => sd.GwIp).Select(sd => sd.GwIp);
        foreach (var ip in ips)
        {
            var gw = _deviceStore.GetGateway(ip);
            if (gw == null)
            {
                if (_options.AutoDiscovery)
                {
                    _logger.LogWarning($"Gateway {ip} not found while in autodiscovery mode. Not updating subdevices. (Try turning off autodiscovery)");
                    return Task.CompletedTask;
                }
                
                gw = new Gateway {IpAddress = ip};
                gw.Name = _options.Gateways.FirstOrDefault(g => g.Ip == gw.IpAddress)?.Name ?? gw.IpAddress.Replace('.','-');
            }

            var devices = message.Subdevices.Where(sd => sd.GwIp == ip);
            foreach (var device in devices)
            {
                var gwDevice = gw.Subdevices.FirstOrDefault(gwsd => gwsd.Id == device.Id);
                if (gwDevice != null)
                {
                    gwDevice.Battery = device.Battery;
                    gwDevice.RfnetState = device.RfnetState;
                    gwDevice.Signal = device.Signal;
                    gwDevice.Ver = device.Ver;
                    gwDevice.TimestampUtc = device.TimestampUtc;
                    gwDevice.Payload = device.Payload;
                
                    _logger.LogInformation($"subdevice updated: {device.Id} ({device.Model})");
                }
                else
                {
                    gw.Subdevices.Add(device);
                    _logger.LogInformation($"subdevice added: {device.Id} ({device.Model})");
                }
            }
            
            _deviceStore.UpsertGateway(gw);
        }
        
        return Task.CompletedTask;
    }
}