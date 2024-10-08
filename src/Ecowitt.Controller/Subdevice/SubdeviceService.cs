using System.Text.Json;
using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Store;
using Microsoft.Extensions.Options;
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
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Stopping SubdeviceService");
        }
    }

    private async Task SubDevicePolling(CancellationToken cancellationToken, bool autoDiscovery = true)
    {
        var subdevices = new SubdeviceApiAggregate();
        if (autoDiscovery)
        {
            // TODO: remove reference to _store and replace it with req/resp through smb
            // TODO: GWxx are Ecowitt models, what about Froggit etc?
            foreach (var gwKvp in _store.GetGatewaysShort().Where(kvp => kvp.Value.StartsWith("GW12") || kvp.Value.StartsWith("GW20")))
            {
                subdevices.Subdevices.AddRange(await GetSubdeviceData(gwKvp.Key, cancellationToken));
            }
        }
        else
        {
            foreach (var gateway in _options.Gateways)
            {
                subdevices.Subdevices.AddRange(await GetSubdeviceData(gateway.Ip, cancellationToken));
            }
        }
        
        if(subdevices.Subdevices.Count > 0) await _messageBus.Publish(subdevices, cancellationToken: cancellationToken);
    }
    
    private async Task<List<SubdeviceApiData>> GetSubdeviceData(string ipAddress, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Polling subdevices from {ipAddress}");
        var subdevices = new List<SubdeviceApiData>();
        try
        {
            subdevices.AddRange(await GetSubdevicesOverview(ipAddress, cancellationToken));
            foreach (var subdevice in subdevices)
            {
                var payload = await GetSubDeviceApiPayload(ipAddress, subdevice.Id, subdevice.Model, cancellationToken);
                subdevice.Payload = payload;
                await Task.Delay(350, cancellationToken); // delay to not overload the gateway
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception while trying to get subdevices from {ipAddress}");
        }
        
        return subdevices;
        
    }

    private async Task <List<SubdeviceApiData>> GetSubdevicesOverview(string ipAddress, CancellationToken cancellationToken)
    {
        var subdevices = new List<SubdeviceApiData>();
        using var client = CreateHttpClient(ipAddress);

        try
        {
            var response = await client.GetAsync("get_iot_device_list", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(content);
                var elements = jsonDocument.RootElement.GetProperty("command");
                foreach (var element in elements.EnumerateArray())
                {
                    
                    var subdevice = new SubdeviceApiData
                    {
                        Id = element.GetProperty("id").GetInt32(),
                        Model = element.GetProperty("model").GetInt32(),
                        Version = element.GetProperty("ver").GetInt32(),
                        RfnetState = element.GetProperty("rfnet_state").GetInt32(),
                        Battery = element.GetProperty("battery").GetInt32(),
                        Signal = element.GetProperty("signal").GetInt32(),
                        GwIp = ipAddress,
                        TimestampUtc = DateTime.UtcNow
                    };
                    _logger.LogInformation($"Subdevice: {subdevice.Id} ({subdevice.Model})");
                    subdevices.Add(subdevice);
                }
            }
            else
            {
                _logger.LogWarning($"Failed to get subdevices from {ipAddress}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception while trying to get subdevices from {ipAddress}");
        }
        

        return subdevices;
    }

    private async Task<string> GetSubDeviceApiPayload(string ipAddress, int subdeviceId, int model, CancellationToken cancellationToken)
    {
        using var client = CreateHttpClient(ipAddress);
        try
        {
            var payload = new { command = new[] { new { cmd = "read_device", id = subdeviceId, model } } };
            var sContent = new StringContent(JsonSerializer.Serialize(payload));
            var response = await client.PostAsync("parse_quick_cmd_iot", sContent, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning($"Could not get payload from {ipAddress} for subdevice {subdeviceId}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception while trying to get payload from {ipAddress} for subdevice {subdeviceId}");
        }
        
        return string.Empty;
    }
    
    private HttpClient CreateHttpClient(string ipAddress)
    {
        var client = _httpClientFactory.CreateClient("ecowitt-client");
        client.BaseAddress = new Uri($"http://{ipAddress}");

        var username = _options.Gateways.FirstOrDefault(gw => gw.Ip == ipAddress)?.Username;
        var password = _options.Gateways.FirstOrDefault(gw => gw.Ip == ipAddress)?.Password;
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
            //TODO: authentication header, need to test
            //client.DefaultRequestHeaders.Add();
        }
        return client;
    }
}