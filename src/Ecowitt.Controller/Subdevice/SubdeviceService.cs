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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting SubdeviceService");
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollingInterval));
        while (await _timer.WaitForNextTickAsync(cancellationToken))
        {   
            // //wild test
            // //{"command":[{"cmd":"read_device","id":10695,"model":2}]}
            // var body = new { command = new { cmd = "read_device", id = 10695, model = 2 }};
            // //var body = $"{{\"command\":[{{\"cmd\":\"read_device\",\"id\":{subdevice.Id},\"model\":{subdevice.Model}}}]}}";
            //              
            // var c = _httpClientFactory.CreateClient();
            // c.BaseAddress = new Uri("http://192.168.103.162/");
            // await c.PostAsJsonAsync("parse_quick_cmd_iot", body, cancellationToken: cancellationToken);
            // //var r = await c.PostAsync("parse_quick_cmd_iot", new StringContent(body, Encoding.UTF8, "application/json"));
            
            
            
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
                            _logger.LogInformation($"subdevice {subdevice.Id} ({subdevice.Model.ToString()}) found on {gateway.Ip}");
                            
                            //{"command":[{"cmd":"read_device","id":10695,"model":2}]}
                            //var body = new { command = new { cmd = "read_device", id = subdevice.Id, model = (int)subdevice.Model }};
                            //var body = $"{{\"command\":[{{\"cmd\":\"read_device\",\"id\":{subdevice.Id},\"model\":{subdevice.Model}}}]}}";
                            
                            //request = await client.PostAsJsonAsync("parse_quick_cmd_iot", body, cancellationToken: cancellationToken);
                            //subdevice.Payload = await r.Content.ReadAsStringAsync(cancellationToken);
                            
                            subdevices.Subdevices.Add(subdevice);
                        }
                    }
                }
            }
            await _messageBus.Publish(subdevices);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        //throw new NotImplementedException();
        _logger.LogInformation("Stopping SubdeviceService");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}