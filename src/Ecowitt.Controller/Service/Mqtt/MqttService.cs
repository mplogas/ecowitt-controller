using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ecowitt.Controller.Message.Config;
using Ecowitt.Controller.Message.Data;
using Ecowitt.Controller.Message.Event;
using Ecowitt.Controller.Model.Api;
using MQTTnet;
using MQTTnet.Client;
using SlimMessageBus;

namespace Ecowitt.Controller.Service.Mqtt;

public partial class MqttService : BackgroundService, IHostedLifecycleService, IConsumer<MqttConfig>, IConsumer<HomeAssistantDiscoveryEvent>, IConsumer<DeviceData>
{
    private readonly ILogger<MqttService> _logger;
    private readonly MqttFactory _factory;
    private readonly IMessageBus _messageBus;
    private IMqttClient? _client;
    private bool _isConnecting;
    private MqttConfig? _mqttConfig;
    private Guid _serviceId = Guid.NewGuid();
    private const string HaStatusTopic = "homeassistant/status";

    public MqttService(ILogger<MqttService> logger, MqttFactory factory, IMessageBus messageBus)
    {
        _logger = logger;
        _factory = factory;
        _messageBus = messageBus;
    }  
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        try
        {
            _logger.LogInformation($"{_serviceId}: handle heartbeat");
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
        if(_client != null && (!_client.IsConnected || _isConnecting))
        {
            _logger.LogWarning("MQTT client is not connected or is currently connecting. Cannot subscribe to Home Assistant state.");
            return;
        }

        await _client.SubscribeAsync(HaStatusTopic);
        await _client.SubscribeAsync($"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceHACommandTopic()}");
        _logger.LogInformation("Subscribed to Home Assistant");
    }
    
    private async Task UnsubscribeHomeAssistant()
    {
        if(_client != null && (!_client.IsConnected || _isConnecting))
        {
            _logger.LogWarning("MQTT client is not connected or is currently connecting. Cannot unsubscribe from Home Assistant state.");
            return;
        }

        await _client.UnsubscribeAsync(HaStatusTopic);
        await _client.UnsubscribeAsync($"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceHACommandTopic()}");
        _logger.LogInformation("Unsubscribed from Home Assistant");
    }

    private void EmitHomeAssistantDiscovery()
    {
        
    }

    private async Task PublishMessage(string topic, dynamic payload)
    {
        if (!await Publish($"{_mqttConfig?.BaseTopic}/{topic}",
                JsonSerializer.Serialize(payload, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping}))) 
            _logger.LogWarning($"Failed to publish message to topic {_mqttConfig?.BaseTopic}/{topic}. Is the client connected?");
    }

    private async Task PublishAvailabilityMessage(string topic, DateTime timestamp)
    {
        var available = DateTime.UtcNow.Subtract(timestamp) < TimeSpan.FromSeconds(300) ? "online" : "offline";

        if (!await Publish($"{_mqttConfig?.BaseTopic}/{topic}/availability", available))
            _logger.LogWarning($"Failed to publish message to topic {_mqttConfig?.BaseTopic}/{topic}. Is the client connected?");
    }

    private async Task<bool> Publish(string topic, string message)
    {
        var mqttPayload = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(message)
            .Build();

        if (_client is { IsConnected: true })
        {
            var result = await _client.PublishAsync(mqttPayload);
            if (result.IsSuccess)
            {
                _logger.LogDebug($"Message {message} published to topic {topic}");
                return true;
            }
            else _logger.LogWarning($"Failed to publish message {message} to topic {topic}. Reason: {result.ReasonCode}");
        }
        else _logger.LogWarning($"Can't publish message {message} to topic {topic}. Client not connected.");
        return false;
    }


}