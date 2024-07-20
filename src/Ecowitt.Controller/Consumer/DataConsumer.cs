using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Mapping;
using Ecowitt.Controller.Store;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Ecowitt.Controller.Consumer;

public class DataConsumer : IConsumer<GatewayApiData>, IConsumer<SubdeviceApiAggregate>
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

    public Task OnHandle(GatewayApiData message)
    {
        _logger.LogInformation($"Received ApiData: {message.PASSKEY}");
        var updatedGateway = message.Map();
        updatedGateway.Name = _options.Gateways.FirstOrDefault(g => g.Ip == updatedGateway.IpAddress)?.Name ?? updatedGateway.IpAddress.Replace('.','-');

        var storedGateway = _deviceStore.GetGateway(updatedGateway.IpAddress);
        if(storedGateway == null)
        {
            updatedGateway.DiscoveryUpdate = true;
            if(!_deviceStore.UpsertGateway(updatedGateway)) _logger.LogWarning($"failed to add gateway {updatedGateway.IpAddress} ({updatedGateway.Model}) to the store");
        }
        else
        {
            // no other property should update besides sensors
            
            foreach (var sensor in updatedGateway.Sensors)
            {
                var storedSensor = storedGateway.Sensors.FirstOrDefault(s => s.Name == sensor.Name);
                if (storedSensor == null)
                {
                    sensor.DiscoveryUpdate = true;
                    storedGateway.Sensors.Add(sensor);
                }
                else
                {
                    storedSensor.Value = sensor.Value; 
                }
            }
            
            var sensorsToRemove = storedGateway.Sensors.Where(s => updatedGateway.Sensors.All(gs => gs.Name != s.Name)).ToList();
            foreach (var sensor in sensorsToRemove)
            {
                storedGateway.Sensors.Remove(sensor);
            }
            
            if(!_deviceStore.UpsertGateway(storedGateway)) _logger.LogWarning($"failed to update {storedGateway.IpAddress} ({storedGateway.Model}) in the store");
        }
        
        return Task.CompletedTask;
    }

    public Task OnHandle(SubdeviceApiAggregate message)
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

            var subdeviceApiData = message.Subdevices.Where(sd => sd.GwIp == ip);
            foreach (var data in subdeviceApiData)
            {
                var updatedSubDevice = data.Map();
                var storedSubDevice = gw.Subdevices.FirstOrDefault(gwsd => gwsd.Id == updatedSubDevice.Id);
                if (storedSubDevice != null)
                {
                    storedSubDevice.TimestampUtc = updatedSubDevice.TimestampUtc;
                    storedSubDevice.Availability = updatedSubDevice.Availability;

                    // no update of other properties
                    if (storedSubDevice.Version != updatedSubDevice.Version || storedSubDevice.Devicename != updatedSubDevice.Devicename || storedSubDevice.Nickname != updatedSubDevice.Nickname)
                    {
                        storedSubDevice.Version = updatedSubDevice.Version;
                        storedSubDevice.Devicename = updatedSubDevice.Devicename;
                        storedSubDevice.Nickname = updatedSubDevice.Nickname;
                        storedSubDevice.DiscoveryUpdate = true;
                    }
                    
                    // update sensors one by one and find out if there are new ones
                    // if there are new ones, mark the subdevice for discovery update
                    foreach (var sensor in updatedSubDevice.Sensors)
                    {
                        var storedSensor = storedSubDevice.Sensors.FirstOrDefault(s => s.Name == sensor.Name);
                        if (storedSensor == null)
                        {
                            sensor.DiscoveryUpdate = true;
                            storedSubDevice.Sensors.Add(sensor);
                        }
                        else
                        {
                            storedSensor.Value = sensor.Value; 
                        }
                    }
                    
                    // remove sensors that are not in the update
                    var sensorsToRemove = storedSubDevice.Sensors.Where(s => updatedSubDevice.Sensors.All(us => us.Name != s.Name)).ToList();
                    foreach (var sensor in sensorsToRemove)
                    {
                        storedSubDevice.Sensors.Remove(sensor);
                    }
                
                    _logger.LogInformation($"subdevice updated: {data.Id} ({data.Model})");
                }
                else
                {
                    gw.Subdevices.Add(updatedSubDevice);
                    _logger.LogInformation($"subdevice added: {data.Id} ({data.Model})");
                }
            }
            
            _deviceStore.UpsertGateway(gw);
        }
        
        return Task.CompletedTask;
    }
}