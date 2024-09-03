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
    private readonly EcowittOptions _ecowittOptions;
    private readonly ControllerOptions _controllerOptions;

    public DataConsumer(ILogger<DataConsumer> logger, IDeviceStore deviceStore, IOptions<EcowittOptions> ecowittOptions, IOptions<ControllerOptions> controllerOptions)
    {
        _logger = logger;
        _deviceStore = deviceStore;
        _ecowittOptions = ecowittOptions.Value;
        _controllerOptions = controllerOptions.Value;
    }

    public Task OnHandle(GatewayApiData message)
    {
        _logger.LogDebug($"Received ApiData: {message.Model} ({message.PASSKEY}) \n {message.Payload}");
        var updatedGateway = message.Map(_controllerOptions.Units == Units.Metric);
        updatedGateway.Name = _ecowittOptions.Gateways.FirstOrDefault(g => g.Ip == updatedGateway.IpAddress)?.Name ?? updatedGateway.IpAddress.Replace('.','-');

        // TODO: der erste storedgateway check schreibt schon sensoren, so dass das discoveryupdate flag nie auf true gesetzt wird

        var storedGateway = _deviceStore.GetGateway(updatedGateway.IpAddress);
        if(storedGateway == null)
        {
            updatedGateway.DiscoveryUpdate = true;
            foreach (var sensor in updatedGateway.Sensors)
            {
                sensor.DiscoveryUpdate = true;
            }
            if(!_deviceStore.UpsertGateway(updatedGateway)) _logger.LogWarning($"failed to add gateway {updatedGateway.IpAddress} ({updatedGateway.Model}) to the store");
        }
        else
        {
            // no other property should update besides sensors 
            // and i'm stupid, because TS is required for availability :(
            storedGateway.TimestampUtc = updatedGateway.TimestampUtc;
            
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
            var storedGateway = _deviceStore.GetGateway(ip);
            if (storedGateway == null)
            {
                if (_ecowittOptions.AutoDiscovery)
                {
                    _logger.LogWarning($"Gateway {ip} not found while in autodiscovery mode. Not updating subdevices. (Try turning off autodiscovery)");
                    return Task.CompletedTask;
                }
                
                storedGateway = new Gateway {IpAddress = ip};
                storedGateway.Name = _ecowittOptions.Gateways.FirstOrDefault(g => g.Ip == storedGateway.IpAddress)?.Name ?? storedGateway.IpAddress.Replace('.','-');
                storedGateway.DiscoveryUpdate = true;
            }

            var subdeviceApiData = message.Subdevices.Where(sd => sd.GwIp == ip);
            foreach (var data in subdeviceApiData)
            {
                var updatedSubDevice = data.Map(_controllerOptions.Units == Units.Metric);
                var storedSubDevice = storedGateway.Subdevices.FirstOrDefault(gwsd => gwsd.Id == updatedSubDevice.Id);
                if (storedSubDevice == null)
                {
                    updatedSubDevice.DiscoveryUpdate = true;
                    foreach (var sensor in updatedSubDevice.Sensors)
                    {
                        sensor.DiscoveryUpdate = true;
                    }
                    storedGateway.Subdevices.Add(updatedSubDevice);
                    _logger.LogInformation($"subdevice added: {data.Id} ({data.Model})");


                } else {
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
            }
            
            _deviceStore.UpsertGateway(storedGateway);
        }
        
        return Task.CompletedTask;
    }
}