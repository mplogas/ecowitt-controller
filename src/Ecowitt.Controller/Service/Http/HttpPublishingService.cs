using Ecowitt.Controller.Message.Config;
using Ecowitt.Controller.Model.Api;
using SlimMessageBus;
using System.Text.Json;

namespace Ecowitt.Controller.Service.Http;

public partial class HttpPublishingService : BackgroundService, IHostedLifecycleService, IConsumer<HttpConfig>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IMessageBus _messageBus;
    private HttpConfig _config = new HttpConfig();

    public HttpPublishingService(ILogger<HttpPublishingService> logger, IMessageBus messageBus, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _messageBus = messageBus;
        _httpClientFactory = httpClientFactory;
    }


    private async Task<List<SubdeviceApiData>> GetSubdeviceData(HttpHost host, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Polling subdevices from {host.Host}");
        var subdevices = new List<SubdeviceApiData>();
        try
        {
            subdevices.AddRange(await GetSubdevicesOverview(host, cancellationToken));
            foreach (var subdevice in subdevices)
            {
                var payload = await GetSubDeviceApiPayload(host, subdevice.Id, subdevice.Model, cancellationToken);
                subdevice.Payload = payload;
                await Task.Delay(350, cancellationToken); // delay to not overload the gateway
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception while trying to get subdevices from {host.Host}");
        }

        return subdevices;

    }

    private async Task<List<SubdeviceApiData>> GetSubdevicesOverview(HttpHost host, CancellationToken cancellationToken)
    {
        var subdevices = new List<SubdeviceApiData>();
        using var client = CreateHttpClient(host);

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
                        GwIp = host.Host,
                        TimestampUtc = DateTime.UtcNow
                    };
                    _logger.LogInformation($"Subdevice: {subdevice.Id} ({subdevice.Model})");
                    subdevices.Add(subdevice);
                }
            }
            else
            {
                _logger.LogWarning($"Failed to get subdevices from {host.Host}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception while trying to get subdevices from {host.Host}");
        }


        return subdevices;
    }

    private async Task<string> GetSubDeviceApiPayload(HttpHost host, int subdeviceId, int model, CancellationToken cancellationToken)
    {
        using var client = CreateHttpClient(host);
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
                _logger.LogWarning($"Could not get payload from {host.Host} for subdevice {subdeviceId}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception while trying to get payload from {host.Host} for subdevice {subdeviceId}");
        }

        return string.Empty;
    }

    private HttpClient CreateHttpClient(HttpHost host)
    {
        var client = _httpClientFactory.CreateClient("ecowitt-client");
        client.BaseAddress = new Uri(host.BaseUrl);

        if (!string.IsNullOrWhiteSpace(host.User) && !string.IsNullOrWhiteSpace(host.Password))
        {
            //TODO: authentication header, need to test
            //client.DefaultRequestHeaders.Add();
        }
        return client;
    }
}