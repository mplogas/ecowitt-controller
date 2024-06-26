using System.Collections.Concurrent;
using Ecowitt.Controller.Model;

namespace Ecowitt.Controller.Store;

public interface IDeviceStore
{
    Gateway? GetGateway(string ipAddress);
    Gateway? GetGatewayBySubdeviceId(int id);
    bool UpsertGateway(Gateway data);
    void Clear();
    Dictionary<string, string> GetGatewaysShort();
}

public class DeviceStore : IDeviceStore
{
    private readonly ConcurrentDictionary<string, Gateway> _gateways = new();
    private readonly ILogger<DeviceStore> _logger;
    
    public DeviceStore(ILogger<DeviceStore> logger)
    {
        _logger = logger;
    }
    
    public Dictionary<string, string> GetGatewaysShort()
    {
        return _gateways.ToArray().ToDictionary(gateway => gateway.Key, gateway => gateway.Value.Model);
    }
    
    public Gateway? GetGateway(string ipAddress)
    {
        return _gateways.TryGetValue(ipAddress, out var gateway) ? gateway : null;
    }

    public Gateway? GetGatewayBySubdeviceId(int id)
    {
        return _gateways.Values.FirstOrDefault(gw => gw.Subdevices.Any(sd => sd.Id == id));
    }
    
    public bool UpsertGateway(Gateway data)
    {
        if (_gateways.TryGetValue(data.IpAddress, out var gateway))
        {
            _logger.LogInformation($"Updateing Gateway {data.IpAddress} ({data.Model})");
            return _gateways.TryUpdate(data.IpAddress, data, gateway);
        }
        else
        {
            _logger.LogInformation($"Adding Gateway {data.IpAddress} ({data.Model})");
            return _gateways.TryAdd(data.IpAddress, data);
        }
    }
    
    public void Clear()
    {
        _gateways.Clear();
    }
}