using System.Collections.Concurrent;
using Ecowitt.Controller.Model;

namespace Ecowitt.Controller.Store;

public interface IDeviceStore
{
    Gateway? GetGateway(string ipAddress);
    bool UpsertGateway(Gateway gateway);
    void Clear();
}

public class DeviceStore : IDeviceStore
{
    private readonly ConcurrentDictionary<string, Gateway> _gateways = new();
    private readonly ILogger<DeviceStore> _logger;
    
    public DeviceStore(ILogger<DeviceStore> logger)
    {
        _logger = logger;
    }
    
    public Gateway? GetGateway(string ipAddress)
    {
        return _gateways.TryGetValue(ipAddress, out var gateway) ? gateway : null;
    }
    
    public bool UpsertGateway(Gateway gateway)
    {
        if (_gateways.ContainsKey(gateway.IpAddress))
        {
            _logger.LogInformation($"Updateing Gateway {gateway.IpAddress} ({gateway.Model})");
            return _gateways.TryUpdate(gateway.IpAddress, gateway, _gateways[gateway.IpAddress]);
        }
        else
        {
            _logger.LogInformation($"Adding Gateway {gateway.IpAddress} ({gateway.Model})");
            return _gateways.TryAdd(gateway.IpAddress, gateway);
        }
    }
    
    public void Clear()
    {
        _gateways.Clear();
    }
}