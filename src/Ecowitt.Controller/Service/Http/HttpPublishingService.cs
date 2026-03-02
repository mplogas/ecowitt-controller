using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Api;
using Ecowitt.Controller.Model.Message.Config;
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
        _logger.LogInformation("Polling subdevices from {HostHost}", host.Host);
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
            _logger.LogError(e, "Exception while trying to get subdevices from {HostHost}", host.Host);
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
                    _logger.LogInformation("Subdevice: {SubdeviceId} ({SubdeviceModel})", subdevice.Id, subdevice.Model);
                    subdevices.Add(subdevice);
                }
            }
            else
            {
                _logger.LogWarning("Failed to get subdevices from {HostHost}", host.Host);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception while trying to get subdevices from {HostHost}", host.Host);
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
                _logger.LogWarning("Could not get payload from {HostHost} for subdevice {SubdeviceId}", host.Host, subdeviceId);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception while trying to get payload from {HostHost} for subdevice {SubdeviceId}", host.Host, subdeviceId);
        }

        return string.Empty;
    }

    public async Task<bool> SendSubdeviceCommand(string gatewayIp, SubdeviceApiCommand command, SubdeviceModel model)
    {
        var host = _config.Hosts.FirstOrDefault(h => h.Host == gatewayIp);
        if (host == null)
        {
            _logger.LogWarning("No host config found for gateway {GatewayIp}", gatewayIp);
            return false;
        }

        using var client = CreateHttpClient(host);
        try
        {
            object payload;
            switch (command.Cmd)
            {
                case Command.Start:
                    var val = command.Duration ?? 0;
                    var valType = (int)(command.Unit ?? DurationUnit.Minutes);
                    var alwaysOn = command.AlwaysOn == true || !command.Duration.HasValue ? 1 : 0;
                    payload = new
                    {
                        command = new[]
                        {
                            new
                            {
                                always_on = alwaysOn, val_type = valType, val,
                                position = 100, cmd = "quick_run",
                                id = command.Id, model = (int)model
                            }
                        }
                    };
                    break;
                case Command.Stop:
                    payload = new { command = new[] { new { cmd = "quick_stop", id = command.Id, model = (int)model } } };
                    break;
                default:
                    _logger.LogWarning("Unsupported command {Cmd} for subdevice {Id}", command.Cmd, command.Id);
                    return false;
            }

            var json = JsonSerializer.Serialize(payload);
            _logger.LogDebug("Sending command payload: {Json}", json);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("parse_quick_cmd_iot", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Sent {Cmd} command to subdevice {Id} on {GatewayIp}", command.Cmd, command.Id, gatewayIp);
                return true;
            }

            _logger.LogWarning("Failed to send command to subdevice {Id} on {GatewayIp}: {StatusCode}", command.Id, gatewayIp, response.StatusCode);
            return false;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception sending command to subdevice {Id} on {GatewayIp}", command.Id, gatewayIp);
            return false;
        }
    }

    private HttpClient CreateHttpClient(HttpHost host)
    {
        var client = _httpClientFactory.CreateClient("ecowitt-client");
        client.BaseAddress = new Uri(host.BaseUrl);

        if (!string.IsNullOrWhiteSpace(host.User) && !string.IsNullOrWhiteSpace(host.Password))
        {
            var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{host.User}:{host.Password}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        }
        return client;
    }
}