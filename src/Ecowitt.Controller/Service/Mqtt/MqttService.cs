using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Api;
using Ecowitt.Controller.Model.Discovery;
using Ecowitt.Controller.Model.Message.Config;
using Ecowitt.Controller.Model.Message.Data;
using Ecowitt.Controller.Model.Message.Event;
using MQTTnet;
using SlimMessageBus;

namespace Ecowitt.Controller.Service.Mqtt;

public partial class MqttService : BackgroundService, IHostedLifecycleService, IConsumer<MqttConfig>, IConsumer<HomeAssistantDiscoveryEvent>, IConsumer<DiscoveryRemovalEvent>, IConsumer<DeviceData>, IConsumer<DeviceDataFull>, IConsumer<SubdeviceData>, IConsumer<SubdeviceDataFull>
{
    private readonly ILogger<MqttService> _logger;
    private readonly MqttClientFactory _factory;
    private readonly IMessageBus _messageBus;
    private IMqttClient? _client;
    private bool _isConnecting;
    private MqttConfig? _mqttConfig;
    private readonly Origin _origin;
    private const string HaStatusTopic = "homeassistant/status";


    public MqttService(ILogger<MqttService> logger, MqttClientFactory factory, IMessageBus messageBus)
    {
        _logger = logger;
        _factory = factory;
        _messageBus = messageBus;
        _origin = DiscoveryBuilder.BuildOrigin();
    }  
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                if (_client is { IsConnected: true } && _mqttConfig != null)
                {
                    await Publish($"{_mqttConfig.BaseTopic}/{_mqttConfig.HeartbeatTopic}",
                        JsonSerializer.Serialize(new { service = DateTime.UtcNow }));
                    await _messageBus.Publish(new MqttServiceEvent { EventType = MqttServiceEventType.Heartbeat }, cancellationToken: stoppingToken);
                    _logger.LogDebug("Sent heartbeat");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Stopping MqttService");
            await _messageBus.Publish(new MqttServiceEvent { EventType = MqttServiceEventType.Stopped }, cancellationToken: stoppingToken);
        }
    }

    

    private async Task SubscribeHomeAssistant()
    {
        if (_client == null || !_client.IsConnected || _isConnecting)
        {
            _logger.LogWarning("MQTT client is not connected or is currently connecting. Cannot subscribe to Home Assistant state.");
            return;
        }

        await _client.SubscribeAsync(HaStatusTopic);
        _logger.LogInformation("Subscribed to Home Assistant");
    }
    
    private async Task UnsubscribeHomeAssistant()
    {
        if (_client == null || !_client.IsConnected || _isConnecting)
        {
            _logger.LogWarning("MQTT client is not connected or is currently connecting. Cannot unsubscribe from Home Assistant state.");
            return;
        }

        await _client.UnsubscribeAsync(HaStatusTopic);
        _logger.LogInformation("Unsubscribed from Home Assistant");
    }

    private async Task EmitHomeAssistantDiscovery(Model.Device gateway)
    {
        await PublishGatewayDiscovery(gateway);
        foreach (var sensor in gateway.Sensors)
        {
            await PublishSensorDiscovery(gateway, sensor);
        }

        foreach (var subdevice in gateway.Subdevices)
        {
            await PublishSubdeviceDiscovery(gateway, subdevice);
            if (subdevice.Model is SubdeviceModel.WFC01 or SubdeviceModel.AC1100 or SubdeviceModel.WFC02)
                await PublishSubdeviceSwitchDiscovery(gateway, subdevice);
            if (subdevice.Model is SubdeviceModel.WFC01 or SubdeviceModel.AC1100 or SubdeviceModel.WFC02)
                await PublishSubdeviceRunModeDiscovery(gateway, subdevice);
            foreach (var sensor in subdevice.Sensors)
            {
                await PublishSensorDiscovery(gateway, subdevice, sensor);
            }
        }
    }

    



}