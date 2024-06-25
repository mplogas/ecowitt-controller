using Ecowitt.Controller.Configuration;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

namespace Ecowitt.Controller.Mqtt;

public interface IMqttClient
{
    Task Connect();
    Task Disconnect();
    Task Subscribe(string topic);
    Task<bool> Publish(string topic, string message);
}

public class MqttMessageReceivedEventArgs : EventArgs
{
    public MqttMessageReceivedEventArgs(string payload, string topic, string clientId)
    {
        Payload = payload;
        Topic = topic;
        ClientId = clientId;
    }

    public string Payload { get; }
    public string Topic { get; }
    public string ClientId { get; }
}

public class MqttClient : IMqttClient, IDisposable
{
    private readonly MQTTnet.Client.IMqttClient _client;
    private readonly ILogger<MqttClient> _logger;
    public EventHandler? OnClientConnected;
    public EventHandler? OnClientDisconnected;
    public EventHandler<MqttMessageReceivedEventArgs>? OnMessageReceived;
    private readonly MqttOptions _options;


    public MqttClient(ILogger<MqttClient> logger, MqttFactory factory, IOptions<MqttOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _client = factory.CreateMqttClient();
        _client.ConnectedAsync += ClientOnConnectedAsync;
        _client.DisconnectedAsync += ClientOnDisconnectedAsync;
        _client.ApplicationMessageReceivedAsync += ClientOnApplicationMessageReceivedAsync;
    }

    public async Task Connect()
    {
        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Host, _options.Port)
            .WithClientId(_options.ClientId)
            .WithCleanSession();
        // allowing empty passwords
        if (!string.IsNullOrWhiteSpace(_options.User)/* && !string.IsNullOrWhiteSpace(_options.Password)*/)
            optionsBuilder.WithCredentials(_options.User, _options.Password);

        if (!_client.IsConnected) await _client.ConnectAsync(optionsBuilder.Build());
        else _logger.LogWarning("Can't connect. Client already connected.");
    }

    public async Task Disconnect()
    {
        if (_client.IsConnected) await _client.DisconnectAsync();
        else _logger.LogWarning("Can't disconnect. Client not connected.");
    }

    public async Task Subscribe(string topic)
    {
        var topicFilter = new MqttTopicFilterBuilder().WithTopic(topic).Build();
        
        if (_client.IsConnected) await _client.SubscribeAsync(topicFilter);
        else _logger.LogWarning($"Can't subscribe to topic {topic}. Client not connected.");
    }

    public async Task<bool> Publish(string topic, string message)
    {
        var mqttPayload = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(message)
            .Build();

        if (_client.IsConnected)
        {
            var result = await _client.PublishAsync(mqttPayload);
            if(result.IsSuccess) return true;
            else _logger.LogWarning($"Failed to publish message {message} to topic {topic}. Reason: {result.ReasonCode}");
        }
        else _logger.LogWarning($"Can't publish message {message} to topic {topic}. Client not connected.");
        return false;
    }

    private Task ClientOnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        var payload = arg.ApplicationMessage.ConvertPayloadToString();
        _logger.LogDebug($"Message received for topic {arg.ApplicationMessage.Topic}: {payload}");

        OnMessageReceived?.Invoke(this,
            new MqttMessageReceivedEventArgs(payload, arg.ApplicationMessage.Topic, arg.ClientId));

        return Task.CompletedTask;
    }

    private async Task ClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        _logger.LogInformation($"MQTT client disconnected. Reason: {arg.Reason}");
        OnClientDisconnected?.Invoke(this, EventArgs.Empty);

        if (_options.Reconnect)
        {
            for (var i = 0; i < _options.ReconnectAttempts; i++)
            {
                try
                {
                    await Connect();
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Reconnect attempt {i + 1} failed.");
                }
            }
        }
        else _logger.LogWarning("Reconnect is disabled, no reconnect attempt");
    }

    private Task ClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        _logger.LogInformation("MQTT client connected.");
        OnClientConnected?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_client.IsConnected) _client.DisconnectAsync().GetAwaiter().GetResult();
        _client.Dispose();
    }
}