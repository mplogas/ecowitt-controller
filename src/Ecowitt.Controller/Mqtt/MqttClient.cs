using MQTTnet;
using MQTTnet.Client;

namespace Ecowitt.Controller.Mqtt
{
    public interface IMqttClient
    {
        Task Connect(string broker, string clientId, string username, string password);
        Task Disconnect();
        Task Subscribe(string topic);
        Task Publish(string topic, string message);
    }

    public class MqttMessageReceivedEventArgs : EventArgs
    {
        public string Payload { get; }
        public string Topic { get; }
        public string ClientId { get; }

        public MqttMessageReceivedEventArgs(string payload, string topic, string clientId)
        {
            this.Payload = payload;
            this.Topic = topic;
            this.ClientId = clientId;
        }
    }

    public class MqttClient : IMqttClient
    {
        public EventHandler<MqttMessageReceivedEventArgs>? OnMessageReceived;
        public EventHandler? OnClientConnected;
        public EventHandler? OnClientDisconnected;

        private readonly MQTTnet.Client.IMqttClient client;
        private readonly ILogger<MqttClient> logger;

        public MqttClient(ILogger<MqttClient> logger, MqttFactory factory)
        {
            this.logger = logger;
            this.client = factory.CreateMqttClient();
            this.client.ConnectedAsync += ClientOnConnectedAsync;
            this.client.DisconnectedAsync += ClientOnDisconnectedAsync;
            this.client.ApplicationMessageReceivedAsync += ClientOnApplicationMessageReceivedAsync;
        }

        public async Task Connect(string broker, string clientId, string username, string password)
        {
            var clientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(broker)
                .WithCredentials(username, password)
                .WithClientId(clientId)
                .Build();

            if (!this.client.IsConnected) await this.client.ConnectAsync(clientOptions);
            else this.logger.LogWarning("Can't connect. Client already connected.");
        }

        public async Task Disconnect()
        {
            if (this.client.IsConnected) await this.client.DisconnectAsync();
            else this.logger.LogWarning("Can't disconnect. Client not connected.");
        }

        public async Task Subscribe(string topic)
        {
            var topicFilter = new MqttTopicFilterBuilder().WithTopic(topic).Build();

            if (this.client.IsConnected) await this.client.SubscribeAsync(topicFilter);
            else this.logger.LogWarning($"Can't subscribe to topic {topic}. Client not connected.");
        }

        public async Task Publish(string topic, string message)
        {
            var mqttPayload = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .Build();

            if (this.client.IsConnected) await this.client.PublishAsync(mqttPayload);
            else this.logger.LogWarning($"Can't publish message {message} to topic {topic}. Client not connected.");
        }

        private Task ClientOnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            var payload = arg.ApplicationMessage.ConvertPayloadToString();
            this.logger.LogDebug($"Message received for topic {arg.ApplicationMessage.Topic}: {payload}");

            this.OnMessageReceived?.Invoke(this, new MqttMessageReceivedEventArgs(payload, arg.ApplicationMessage.Topic, arg.ClientId));

            return Task.CompletedTask;
        }

        private Task ClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            this.logger.LogDebug($"Client disconnected. Reason: {arg.ReasonString}");
            this.OnClientDisconnected?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        private Task ClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            this.logger.LogDebug("Client connected.");
            this.OnClientConnected?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
    }
}
