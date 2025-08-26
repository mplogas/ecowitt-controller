using System.Text.Json;
using Ecowitt.Controller.Model.Api;
using Ecowitt.Controller.Model.Configuration;
using Ecowitt.Controller.Model.Message.Config;
using Ecowitt.Controller.Model.Message.Data;
using Ecowitt.Controller.Model.Message.Event;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Ecowitt.Controller.Service.Orchestrator;

public partial class StateMachine : BackgroundService, IConsumer<MqttServiceEvent>, IConsumer<MqttConnectionEvent>, IConsumer<HomeAssistantStatusEvent>, IConsumer<SubdeviceApiCommand>, IConsumer<GatewayApiData>, IConsumer<SubdeviceApiAggregate>
{
    private readonly ILogger<StateMachine> _logger;
    private readonly IDeviceStore _deviceStore;
    private readonly EcowittOptions _ecowittOptions;
    private readonly ControllerOptions _controllerOptions;
    private readonly MqttOptions _mqttOptions;
    private readonly IMessageBus _messageBus;
    private MqttServiceEventType _lastMqttServiceState = MqttServiceEventType.Unknown;
    private HttpServiceEventType _lastHttpServiceState = HttpServiceEventType.Unknown;

    public StateMachine(ILogger<StateMachine> logger, IDeviceStore deviceStore, IMessageBus messageBus, IOptions<MqttOptions> mqttOptions, IOptions<EcowittOptions> ecowittOptions, IOptions<ControllerOptions> controllerOptions) 
    {
        _logger = logger;
        _deviceStore = deviceStore;
        _mqttOptions = mqttOptions.Value;
        _ecowittOptions = ecowittOptions.Value;
        _controllerOptions = controllerOptions.Value;
        _messageBus = messageBus;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Orchestrator");

        _logger.LogInformation("Emitting initial configuration");
        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        await EmitMqttConfig();
        await EmitHttpConfig();
    }

    private async Task EmitHttpConfig()
    {
        var httpConfig = new HttpConfig
        {
            Hosts = _ecowittOptions.Gateways.Select(gw => new HttpHost(gw.Ip, gw.Username, gw.Password)).ToList(),
            PollingInterval = _ecowittOptions.PollingInterval,
            AutoDiscovery = _ecowittOptions.AutoDiscovery
        };

        await _messageBus.Publish(httpConfig);
    }

    private async Task EmitMqttConfig()
    {
        var mqttConfig = new MqttConfig
        {
            Host = _mqttOptions.Host,
            Port = _mqttOptions.Port,
            ClientId = _mqttOptions.ClientId,
            BaseTopic = _mqttOptions.BaseTopic,
            ReconnectAttempts = _mqttOptions.ReconnectAttempts,
            HomeAssistantDiscovery = _controllerOptions.HomeAssistantDiscovery,
            Precision = _controllerOptions.Precision,
            PublishingInterval = _controllerOptions.PublishingInterval,
            Units = _controllerOptions.Units
        };
        if (!string.IsNullOrWhiteSpace(_mqttOptions.User))
        {
            mqttConfig.User = _mqttOptions.User;
            mqttConfig.Password = _mqttOptions.Password;
        }

        await _messageBus.Publish(mqttConfig);
    }

    private void LogStorageState()
    {
        foreach (var gw in _deviceStore.GetGatewaysShort())
        {
            _logger.LogInformation($"Gateway {gw.Key} - {gw.Value}");
            var gateway = _deviceStore.GetGateway(gw.Key);
            _logger.LogDebug($"Storage dump: \n {JsonSerializer.Serialize(gateway)}");
        }
    }
    
    // public async Task<bool> SendCommand(string ipAddress, string cmd, int id, int model, int val = 0, int valType = 0, int onType = 0, int offType = 0, int alwaysOn = 1, int onTime = 0, int offTime = 0)
    // {
    //     var client = _httpClientFactory.CreateClient("ecowitt-client");
    //     client.BaseAddress = new Uri($"http://{ipAddress}");
    //
    //     var username = _ecowittOptions.Gateways.FirstOrDefault(gw => gw.Ip == ipAddress)?.Username;
    //     var password = _ecowittOptions.Gateways.FirstOrDefault(gw => gw.Ip == ipAddress)?.Password;
    //     if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
    //     {
    //         //TODO: authentication header, need to test
    //         //client.DefaultRequestHeaders.Add();
    //     }
    //     
    //     // [{"on_type":0,"off_type":0,"always_on":0,"on_time":0,"off_time":0,"val_type":1,"val":20,"cmd":"quick_run","id":12345,"model":1}]}    
    //     dynamic payload;
    //     switch (cmd)
    //     {
    //         case "quick_run":
    //             payload = new { command = new[] { new { cmd, id, model, val, val_type = valType, on_type = onType, off_type = offType, always_on = alwaysOn, on_time = onTime, off_time = offTime } } };
    //             break;
    //         case "quick_stop":
    //             payload = new { command = new[] { new { cmd, id, model } } };
    //             break;
    //         default:
    //             _logger.LogWarning($"Unsupported command type {cmd}. Not sending command to {ipAddress} for subdevice {id}");
    //             return false;
    //     }
    //     
    //     var sContent = new StringContent(JsonSerializer.Serialize(payload));
    //     var response = await client.PostAsync("parse_quick_cmd_iot", sContent);
    //
    //     if (response.IsSuccessStatusCode) return true;
    //     else {
    //         _logger.LogWarning($"Could not send command to {ipAddress} for subdevice {id}");
    //         return false;
    //     }
    // }

}