using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Message.Config;
using Ecowitt.Controller.Message.Data;
using Ecowitt.Controller.Message.Event;
using Ecowitt.Controller.Model;
using MQTTnet;
using MQTTnet.Client;
using SlimMessageBus;

namespace Ecowitt.Controller.Service.Mqtt;

public class MqttService : BackgroundService, IConsumer<MqttConfig>, IConsumer<HomeAssistantConfig>, IConsumer<HomeAssistantDiscoveryEvent>, IConsumer<DeviceData>
{
    private readonly ILogger<MqttService> _logger;
    private readonly MqttFactory _factory;
    private readonly IMessageBus _messageBus;
    private IMqttClient _client;
    private bool _homeAssistantDiscoveryEnabled;
    private bool _isConnecting = false;
    private MqttConfig _mqttConfig = new ();
    

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
            if(_client.IsConnected) await _client.DisconnectAsync();
            _client.ApplicationMessageReceivedAsync -= ClientOnApplicationMessageReceivedAsync;
            _client.DisconnectedAsync -= ClientOnDisconnectedAsync;
            _client.ConnectedAsync -= ClientOnConnectedAsync;
            _client.Dispose();
        }
        
        if(!message.Enabled) 
        {
            _logger.LogInformation("MQTT is disabled");
            return;
        }

        _mqttConfig = message;
        _client = _factory.CreateMqttClient();
        _client.ConnectedAsync += ClientOnConnectedAsync;
        _client.DisconnectedAsync += ClientOnDisconnectedAsync;
        _client.ApplicationMessageReceivedAsync += ClientOnApplicationMessageReceivedAsync;
        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(_mqttConfig.Configuration.Host, _mqttConfig.Configuration.Port)
            .WithClientId(_mqttConfig.Configuration.ClientId)
            .WithCleanSession();
        if(!string.IsNullOrWhiteSpace(_mqttConfig.Configuration.User))
            optionsBuilder.WithCredentials(_mqttConfig.Configuration.User, _mqttConfig.Configuration.Password);
        
        await _client.ConnectAsync(optionsBuilder.Build());
        await _client.SubscribeAsync($"{_mqttConfig.Configuration.BaseTopic}/{Helper.BuildMqttSubdeviceCommandTopic()}");
    }

    public async Task OnHandle(HomeAssistantConfig message)
    {
        _homeAssistantDiscoveryEnabled = message.Enabled;
        if (_homeAssistantDiscoveryEnabled)
        {
            SubscribeHomeAssistantState();
        } else 
        {
            UnsubscribeHomeAssistantState();
        }
    }

    public async Task OnHandle(HomeAssistantDiscoveryEvent message)
    {
        EmitHomeAssistantDiscovery();
    }

    public async Task OnHandle(DeviceData message)
    {
        if (_client == null || !_client.IsConnected)
        {
            _logger.LogWarning("MQTT client is not connected. Cannot publish device data.");
            return;
        }

        
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }

    private Task ClientOnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        throw new NotImplementedException();
    }

    private async Task ClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        _logger.LogInformation("MQTT client disconnected.");
        _messageBus.Publish<MqttConnectionEvent>(new MqttConnectionEvent { EventType = EventType.Disconnected });
        if (_mqttConfig.Configuration.Reconnect && !_isConnecting)
        {
            _logger.LogInformation("Attempting to reconnect to MQTT broker...");
            _isConnecting = true;
            try
            {
                for (int i = 0; i < _mqttConfig.Configuration.ReconnectAttempts; i++)
                {
                    try
                    {
                        var result = await _client.ConnectAsync(_client.Options);
                        if(result.ResultCode == MqttClientConnectResultCode.Success) break;
                        else _logger.LogWarning($"Failed to reconnect to MQTT broker. Attempt {i + 1} of {_mqttConfig.Configuration.ReconnectAttempts}. Reason: {result.ResultCode}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception during MQTT reconnect attempt {i + 1} of {_mqttConfig.Configuration.ReconnectAttempts}: {ex.Message}");
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

    private Task ClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        _logger.LogInformation("MQTT client connected.");
        _messageBus.Publish<MqttConnectionEvent>(new MqttConnectionEvent
        {
            EventType = EventType.Connected
        });
        
        return Task.CompletedTask;
    }

    private void SubscribeHomeAssistantState()
    {
        if(!_client.IsConnected || _isConnecting)
        {
            _logger.LogWarning("MQTT client is not connected or is currently connecting. Cannot subscribe to Home Assistant state.");
            return;
        }
        
        _client.SubscribeAsync($"{_mqttConfig.Configuration.BaseTopic}/{Helper.BuildMqttSubdeviceHACommandTopic()}");
        _logger.LogInformation("Subscribed to Home Assistant state topic: {Topic}", $"{_mqttConfig.Configuration.BaseTopic}/{Helper.BuildMqttSubdeviceHACommandTopic()}");
    }
    
    private void UnsubscribeHomeAssistantState()
    {
        if(!_client.IsConnected || _isConnecting)
        {
            _logger.LogWarning("MQTT client is not connected or is currently connecting. Cannot unsubscribe from Home Assistant state.");
            return;
        }
        
        _client.UnsubscribeAsync($"{_mqttConfig.Configuration.BaseTopic}/{Helper.BuildMqttSubdeviceHACommandTopic()}");
        _logger.LogInformation("Unsubscribed from Home Assistant state topic: {Topic}", $"{_mqttConfig.Configuration.BaseTopic}/{Helper.BuildMqttSubdeviceHACommandTopic()}");
    }

    private void EmitHomeAssistantDiscovery()
    {
        
    }
    
    private dynamic BuildSubdevicePayload(Model.Subdevice subdevice)
    {
        return new
        {
            id = subdevice.Id,
            model = subdevice.Model,
            devicename = subdevice.Devicename,
            nickname = subdevice.Nickname,
            state = subdevice.Availability ? "online" : "offline",
            ver = subdevice.Version
        };
    }

    private dynamic BuildSensorPayload(ISensor s)
    {
        _logger.LogDebug($"Sensor {s.Name} datatype: {s.DataType}");
        return new
        {
            name = s.Name,
            alias = s.Alias,
            value = s.DataType == typeof(double) ? Math.Round(Convert.ToDouble(s.Value), _controllerOptions.Precision) : s.Value,
            unit = !string.IsNullOrWhiteSpace(s.UnitOfMeasurement) ? s.UnitOfMeasurement : null
            //type = s.SensorType != SensorType.None ? s.SensorType.ToString() : null
        };
    }                        

    private dynamic BuildGatewayPayload(Device gw)
    {
        if (string.IsNullOrWhiteSpace(gw.Model))
        {
            return new
            {
                ip = gw.IpAddress,
                name = gw.Name
            };
        }
        
        return new
        {
            ip = gw.IpAddress,
            name = gw.Name,
            model = gw.Model,
            passkey = gw.PASSKEY,
            stationType = gw.StationType,
            runtime = gw.Runtime,
            state = (DateTime.UtcNow - gw.TimestampUtc).TotalSeconds < _controllerOptions.PublishingInterval * 3 ? "online" : "offline",
            freq = gw.Freq
        };
    }

    private async Task PublishMessage(string topic, dynamic payload)
    {
        if (!await _mqttClient.Publish($"{_mqttOptions.BaseTopic}/{topic}",
                JsonSerializer.Serialize(payload, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping}))) 
            _logger.LogWarning($"Failed to publish message to topic {_mqttOptions.BaseTopic}/{topic}. Is the client connected?");
    }

    private async Task PublishAvailabilityMessage(string topic, DateTime timestamp)
    {
        var available = DateTime.UtcNow.Subtract(timestamp) < TimeSpan.FromSeconds(300) ? "online" : "offline";

        if (!await _mqttClient.Publish($"{_mqttOptions.BaseTopic}/{topic}/availability", available))
            _logger.LogWarning($"Failed to publish message to topic {_mqttOptions.BaseTopic}/{topic}. Is the client connected?");
    }

}