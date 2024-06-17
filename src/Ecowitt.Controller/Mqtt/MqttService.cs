using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Model;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Ecowitt.Controller.Mqtt
{
    public class MqttService : IHostedService
    {
        private readonly ILogger<MqttService> _logger;
        private readonly MqttClient _mqttClient;
        private readonly IOptions<MqttOptions> _mqttConfig;
        private readonly IMessageBus _messageBus;
        private const string CmdTopic = "cmd";
        private const string HeartbeatTopic = "heartbeat";

        public MqttService(IOptions<MqttOptions> config, ILogger<MqttService> logger, IMqttClient mqttClient, IMessageBus messageBus)
        {
            this._logger = logger;
            this._mqttConfig = config;
            this._messageBus = messageBus;
            
            this._mqttClient = (MqttClient)mqttClient; //TODO: i feel bad for the hard cast
            this._mqttClient.OnMessageReceived += OnMessageReceived;
            this._mqttClient.OnClientDisconnected += OnClientDisconnected;
            this._mqttClient.OnClientConnected += OnClientConnected;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await this._mqttClient.Connect(_mqttConfig.Value.Host, _mqttConfig.Value.ClientId, _mqttConfig.Value.User, _mqttConfig.Value.Password);
            this._logger.LogInformation("Connected");

            await _mqttClient.Subscribe($"{_mqttConfig.Value.BaseTopic}/{CmdTopic}/#");
            this._logger.LogInformation("subscribed to all topics");
            
            //emit a heartbeat every 30 seconds
            
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this._mqttClient.Disconnect();
            this._logger.LogInformation("Disconnected");
        }

        private void OnClientConnected(object sender, EventArgs e)
        {
            this._logger.LogInformation("onconnect");
        }

        private void OnClientDisconnected(object sender, EventArgs e)
        {
            this._logger.LogInformation("ondisconnect");
        }

        private async void OnMessageReceived(object sender, MqttMessageReceivedEventArgs e)
        {
            this._logger.LogInformation("Received message on topic {Topic} with payload {Payload}", e.Topic, e.Payload);
            this._messageBus.Publish(new SubdeviceCommand()
                { Cmd = "test", Id = 12345, Model = 2, Payload = string.Empty });
        }
    }
}
