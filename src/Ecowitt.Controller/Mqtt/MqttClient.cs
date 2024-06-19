using MQTTnet;
using MQTTnet.Client;

namespace Ecowitt.Controller.Mqtt;

public interface IMqttClient
{
    Task Connect(string broker, string clientId, string username, string password);
    Task Disconnect();
    Task Subscribe(string topic);
    Task Publish(string topic, string message);
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

public class MqttClient : IMqttClient
{
    private readonly MQTTnet.Client.IMqttClient client;
    private readonly ILogger<MqttClient> logger;
    public EventHandler? OnClientConnected;
    public EventHandler? OnClientDisconnected;
    public EventHandler<MqttMessageReceivedEventArgs>? OnMessageReceived;

    public MqttClient(ILogger<MqttClient> logger, MqttFactory factory)
    {
        this.logger = logger;
        client = factory.CreateMqttClient();
        client.ConnectedAsync += ClientOnConnectedAsync;
        client.DisconnectedAsync += ClientOnDisconnectedAsync;
        client.ApplicationMessageReceivedAsync += ClientOnApplicationMessageReceivedAsync;
    }

    public async Task Connect(string broker, string clientId, string username, string password)
    {
        var clientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(broker)
            .WithCredentials(username, password)
            .WithClientId(clientId)
            .Build();

        if (!client.IsConnected) await client.ConnectAsync(clientOptions);
        else logger.LogWarning("Can't connect. Client already connected.");
    }

    public async Task Disconnect()
    {
        if (client.IsConnected) await client.DisconnectAsync();
        else logger.LogWarning("Can't disconnect. Client not connected.");
    }

    public async Task Subscribe(string topic)
    {
        var topicFilter = new MqttTopicFilterBuilder().WithTopic(topic).Build();

        if (client.IsConnected) await client.SubscribeAsync(topicFilter);
        else logger.LogWarning($"Can't subscribe to topic {topic}. Client not connected.");
    }

    public async Task Publish(string topic, string message)
    {
        var mqttPayload = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(message)
            .Build();

        if (client.IsConnected) await client.PublishAsync(mqttPayload);
        else logger.LogWarning($"Can't publish message {message} to topic {topic}. Client not connected.");
    }

    private Task ClientOnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        var payload = arg.ApplicationMessage.ConvertPayloadToString();
        logger.LogDebug($"Message received for topic {arg.ApplicationMessage.Topic}: {payload}");

        OnMessageReceived?.Invoke(this,
            new MqttMessageReceivedEventArgs(payload, arg.ApplicationMessage.Topic, arg.ClientId));

        return Task.CompletedTask;
    }

    private Task ClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        logger.LogDebug($"Client disconnected. Reason: {arg.ReasonString}");
        OnClientDisconnected?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    private Task ClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        logger.LogDebug("Client connected.");
        OnClientConnected?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}