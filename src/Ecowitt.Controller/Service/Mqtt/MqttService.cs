using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ecowitt.Controller.Message.Config;
using Ecowitt.Controller.Message.Data;
using Ecowitt.Controller.Message.Event;
using MQTTnet;
using MQTTnet.Client;
using SlimMessageBus;

namespace Ecowitt.Controller.Service.Mqtt;

public class MqttService : BackgroundService, IConsumer<MqttConfig>, IConsumer<HomeAssistantDiscoveryEvent>, IConsumer<DeviceData>
{
    private readonly ILogger<MqttService> _logger;
    private readonly MqttFactory _factory;
    private readonly IMessageBus _messageBus;
    private IMqttClient? _client;
    private bool _isConnecting;
    private MqttConfig? _mqttConfig;
    

    public MqttService(ILogger<MqttService> logger, MqttFactory factory, IMessageBus messageBus)
    {
        _logger = logger;
        _factory = factory;
        _messageBus = messageBus;
    }  
    
    public async Task OnHandle(MqttConfig message)
    {
        if(_client != null)
        {
            try
            {
                if (_mqttConfig is { HomeAssistantDiscovery: true }) UnsubscribeHomeAssistantState();
                
                if(_client.IsConnected) await _client.DisconnectAsync();
                _client.ApplicationMessageReceivedAsync -= ClientOnApplicationMessageReceivedAsync;
                _client.DisconnectedAsync -= ClientOnDisconnectedAsync;
                _client.ConnectedAsync -= ClientOnConnectedAsync;
                _client.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while disconnecting, unregistering or disposing MQTT client.");
                return;
            }
        }
        
        _mqttConfig = message;
        
        if(!message.Enabled) 
        {
            _logger.LogInformation("MQTT is disabled");
            return;
        }

        _client = _factory.CreateMqttClient();
        _client.ConnectedAsync += ClientOnConnectedAsync;
        _client.DisconnectedAsync += ClientOnDisconnectedAsync;
        _client.ApplicationMessageReceivedAsync += ClientOnApplicationMessageReceivedAsync;
        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(_mqttConfig.Host, _mqttConfig.Port)
            .WithClientId(_mqttConfig.ClientId)
            .WithCleanSession();
        if(!string.IsNullOrWhiteSpace(_mqttConfig.User))
            optionsBuilder.WithCredentials(_mqttConfig.User, _mqttConfig.Password);
        
        await _client.ConnectAsync(optionsBuilder.Build());
        await _client.SubscribeAsync($"{_mqttConfig.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceCommandTopic()}");
        
        if(_mqttConfig.HomeAssistantDiscovery) SubscribeHomeAssistantState();
    }

    public async Task OnHandle(HomeAssistantDiscoveryEvent message)
    {
        EmitHomeAssistantDiscovery();
    }

    public Task OnHandle(DeviceData message)
    {
        if (_client == null || !_client.IsConnected)
        {
            _logger.LogWarning("MQTT client is not connected. Cannot publish device data.");
        }

        return Task.CompletedTask;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting MqttService");
        
        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(60));
        try
        {
            while(await timer.WaitForNextTickAsync(stoppingToken))
            {
                if (_client is { IsConnected: true } && _mqttConfig != null)
                {
                    await Publish($"{_mqttConfig.BaseTopic}/{_mqttConfig.HeartbeatTopic}",
                        JsonSerializer.Serialize(new { service = DateTime.UtcNow }));
                    _logger.LogInformation("Sent heartbeat");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Stopping MqttService");
        }
    }

    private Task ClientOnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        var payload = arg.ApplicationMessage.ConvertPayloadToString();
        var topic = arg.ApplicationMessage.Topic;
        _logger.LogDebug($"Message received for topic {topic}: {payload}");
        
        if (topic.EndsWith("homeassistant"))
        {
            if (int.TryParse(topic.Split('/')[3], out var result))
            {
                //var cmd = payload.Equals("ON", StringComparison.InvariantCultureIgnoreCase) ? Command.Start : Command.Stop; //I know, everything that's not "ON" is "OFF"
                //await _messageBus.Publish(new SubdeviceApiCommand() { Cmd = cmd, Id = result });
            }
            else
            {
                _logger.LogWarning("Invalid subdevice id in topic {Topic}", topic);
            }
        }
        else
        {
            // var cmd = JsonSerializer.Deserialize<SubdeviceApiCommand>(e.Payload);
            // await _messageBus.Publish(cmd);
        }

        return Task.CompletedTask;
        
    }

    private async Task ClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        _logger.LogInformation("MQTT client disconnected.");
        await _messageBus.Publish<MqttConnectionEvent>(new MqttConnectionEvent { EventType = EventType.Disconnected });
        if (_mqttConfig is { Reconnect: true } && !_isConnecting)
        {
            _logger.LogInformation("Attempting to reconnect to MQTT broker...");
            _isConnecting = true;
            try
            {
                for (var i = 0; i < _mqttConfig.ReconnectAttempts; i++)
                {
                    try
                    {
                        var result = await _client?.ConnectAsync(_client.Options)!;
                        if(result.ResultCode == MqttClientConnectResultCode.Success) break;
                        else _logger.LogWarning($"Failed to reconnect to MQTT broker. Attempt {i + 1} of {_mqttConfig.ReconnectAttempts}. Reason: {result.ResultCode}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception during MQTT reconnect attempt {i + 1} of {_mqttConfig.ReconnectAttempts}: {ex.Message}");
                    }
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
            finally
            {
                _isConnecting = false;
            }
        }
    }

    private async Task ClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        _logger.LogInformation("MQTT client connected.");
        await _messageBus.Publish<MqttConnectionEvent>(new MqttConnectionEvent
        {
            EventType = EventType.Connected
        });
    }

    private void SubscribeHomeAssistantState()
    {
        if(_client != null && (!_client.IsConnected || _isConnecting))
        {
            _logger.LogWarning("MQTT client is not connected or is currently connecting. Cannot subscribe to Home Assistant state.");
            return;
        }
        
        _client.SubscribeAsync($"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceHACommandTopic()}");
        _logger.LogInformation("Subscribed to Home Assistant state topic: {Topic}", $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceHACommandTopic()}");
    }
    
    private void UnsubscribeHomeAssistantState()
    {
        if(_client != null && (!_client.IsConnected || _isConnecting))
        {
            _logger.LogWarning("MQTT client is not connected or is currently connecting. Cannot unsubscribe from Home Assistant state.");
            return;
        }
        
        _client.UnsubscribeAsync($"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceHACommandTopic()}");
        _logger.LogInformation("Unsubscribed from Home Assistant state topic: {Topic}", $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceHACommandTopic()}");
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