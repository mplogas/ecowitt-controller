using System.Net.Http;
using System.Text;
using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SlimMessageBus;

namespace Ecowitt.Controller.Subdevice;

internal class SubdeviceService : IHostedService, IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IMessageBus _messageBus;
    private readonly EcowittOptions _options;
    private PeriodicTimer? _timer;
    
    public SubdeviceService(ILogger<SubdeviceService> logger, IMessageBus messageBus,
        IHttpClientFactory httpClientFactory, IOptions<EcowittOptions> options)
    {
        _logger = logger;
        _messageBus = messageBus;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting SubdeviceService");
        Task.Run(async () =>
        {
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollingInterval));
            while (await _timer.WaitForNextTickAsync(cancellationToken) && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Polling subdevices");
                var subdevices = new SubdeviceData();
                foreach (var gateway in _options.Gateways)
                {
                    using var client = _httpClientFactory.CreateClient($"ecowitt-client-{gateway.Name}");
                    var response = await client.GetAsync("get_iot_device_list", cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        dynamic data = JsonConvert.DeserializeObject(content);
                        if (data is not null && data.command is not null)
                        {
                            //[{"cmd":"read_quick","model":1,"id":13441,"ver":113,"rfnet_state":1,"battery":5,"signal":4},
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
                                    GwIp = gateway.Ip
                                };
                                _logger.LogInformation(
                                    $"subdevice {subdevice.Id} ({subdevice.Model.ToString()}) found on {gateway.Ip}");

                                //{"command":[{"cmd":"read_device","id":10695,"model":2}]}
                                var payload = new
                                {
                                    command = new[]
                                        { new { cmd = "read_device", id = subdevice.Id, model = subdevice.Model } }
                                };
                                var sContent = new StringContent(JsonConvert.SerializeObject(payload));
                                response = await client.PostAsync("parse_quick_cmd_iot", sContent, cancellationToken);
                                if (response.IsSuccessStatusCode)
                                {
                                    var c = await response.Content.ReadAsStringAsync(cancellationToken);
                                    if (!string.IsNullOrWhiteSpace(c))
                                    {
                                        var s = await response.Content.ReadAsStringAsync(cancellationToken);
                                        _logger.LogInformation(
                                            $"payload received for {subdevice.Id} ({subdevice.Model.ToString()}): {s}");
                                        subdevice.Payload = s;
                                    }
                                }

                                subdevices.Subdevices.Add(subdevice);
                            }
                        }
                    }
                }

                await _messageBus.Publish(subdevices, cancellationToken: cancellationToken);
            }
        }, cancellationToken);

        return Task.CompletedTask;

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping SubdeviceService");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}