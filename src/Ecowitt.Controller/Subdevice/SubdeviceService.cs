using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Store;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SlimMessageBus;

namespace Ecowitt.Controller.Subdevice;

public class SubdeviceService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IMessageBus _messageBus;
    private readonly EcowittOptions _options;
    private readonly IDeviceStore _store;

    public SubdeviceService(ILogger<SubdeviceService> logger, IMessageBus messageBus,
        IHttpClientFactory httpClientFactory, IDeviceStore store, IOptions<EcowittOptions> options)
    {
        _logger = logger;
        _messageBus = messageBus;
        _httpClientFactory = httpClientFactory;
        _store = store;
        _options = options.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting SubdeviceService");
        
        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollingInterval));
        try
        {
            while(await timer.WaitForNextTickAsync(stoppingToken))
            {
                await SubDevicePolling(stoppingToken, _options.AutoDiscovery);
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.LogInformation("Stopping SubdeviceService");
        }
    }

    private async Task SubDevicePolling(CancellationToken cancellationToken, bool autoDiscovery = true)
    {
        _logger.LogInformation("Polling subdevices");
        var subdevices = new SubdeviceData();

        if (autoDiscovery)
        {
            foreach (var gw_kwp in _store.GetGatewaysShort().Where(gw_kwp => !gw_kwp.Value.StartsWith("GW1000A")))
            {
                subdevices.Subdevices.AddRange(await GetSubdeviceData(gw_kwp.Key, cancellationToken));
            }
        }
        else
        {
            foreach (var gateway in _options.Gateways)
            {
                subdevices.Subdevices.AddRange(await GetSubdeviceData(gateway.Ip, cancellationToken));
            }
        }
        
        await _messageBus.Publish(subdevices, cancellationToken: cancellationToken);
    }
    
    private async Task<List<Model.Subdevice>> GetSubdeviceData(string ipAddress, CancellationToken cancellationToken, string username = "", string password = "")
    {
        var subdevices = new List<Model.Subdevice>();
        using var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri($"http://{ipAddress}");
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
            // authentication header, need to test
            //client.DefaultRequestHeaders.Add();
        }

        try
        {
            var response = await client.GetAsync("get_iot_device_list", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(content);
                if (data is not null && data.command is not null)
                {
                    foreach (var device in data.command)
                    {
                        var subdevice = new Model.Subdevice
                        {
                            Id = device.id,
                            Model = device.model,
                            Ver = device.ver,
                            RfnetState = device.rfnet_state,
                            Battery = device.battery,
                            Signal = device.signal,
                            GwIp = ipAddress,
                            TimestampUtc = DateTime.UtcNow
                        };
                        _logger.LogInformation($"Subdevice: {subdevice.Id} ({subdevice.Model})");
                        subdevices.Add(subdevice);
                    }
                }
            }
            else
            {
                _logger.LogWarning($"Failed to get subdevices from {ipAddress}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception while tryint to get subdevices from {ipAddress}");
        }
        

        return subdevices;
    }
}