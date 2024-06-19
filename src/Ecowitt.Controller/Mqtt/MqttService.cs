using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Model;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Ecowitt.Controller.Mqtt;

public class MqttService : IHostedService
{
    private const string CmdTopic = "cmd";
    private const string HeartbeatTopic = "heartbeat";
    private readonly ILogger<MqttService> _logger;
    private readonly IMessageBus _messageBus;
    private readonly MqttClient _mqttClient;
    private readonly IOptions<MqttOptions> _mqttConfig;

    public MqttService(IOptions<MqttOptions> config, ILogger<MqttService> logger, IMqttClient mqttClient,
        IMessageBus messageBus)
    {
        _logger = logger;
        _mqttConfig = config;
        _messageBus = messageBus;

        _mqttClient = (MqttClient)mqttClient; //TODO: i feel bad for the hard cast
        _mqttClient.OnMessageReceived += OnMessageReceived;
        _mqttClient.OnClientDisconnected += OnClientDisconnected;
        _mqttClient.OnClientConnected += OnClientConnected;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _mqttClient.Connect(_mqttConfig.Value.Host, _mqttConfig.Value.ClientId, _mqttConfig.Value.User,
            _mqttConfig.Value.Password);
        _logger.LogInformation("Connected");

        await _mqttClient.Subscribe($"{_mqttConfig.Value.BaseTopic}/{CmdTopic}/#");
        _logger.LogInformation("subscribed to all topics");

        //emit a heartbeat every 30 seconds
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _mqttClient.Disconnect();
        _logger.LogInformation("Disconnected");
    }

    private void OnClientConnected(object sender, EventArgs e)
    {
        _logger.LogInformation("onconnect");
    }

    private void OnClientDisconnected(object sender, EventArgs e)
    {
        _logger.LogInformation("ondisconnect");
    }

    private async void OnMessageReceived(object sender, MqttMessageReceivedEventArgs e)
    {
        _logger.LogInformation("Received message on topic {Topic} with payload {Payload}", e.Topic, e.Payload);
        _messageBus.Publish(new SubdeviceCommand { Cmd = "test", Id = 12345, Model = 2, Payload = string.Empty });
    }
}