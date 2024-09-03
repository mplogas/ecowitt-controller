using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SlimMessageBus;

namespace Ecowitt.Controller.Mqtt;

public class MqttService : BackgroundService
{
    private const string CmdTopic = "cmd";
    private const string HeartbeatTopic = "heartbeat";
    private readonly ILogger<MqttService> _logger;
    private readonly IMessageBus _messageBus;
    private readonly IMqttClient _mqttClient;
    private readonly MqttOptions _mqttConfig;
    private readonly ControllerOptions _controllerConfig;

    public MqttService(IOptions<MqttOptions> mqttConfig, IOptions<ControllerOptions> controllerConfig, ILogger<MqttService> logger, IMqttClient mqttClient,
        IMessageBus messageBus)
    {
        _logger = logger;
        _mqttConfig = mqttConfig.Value;
        _controllerConfig = controllerConfig.Value;
        _messageBus = messageBus;

        _mqttClient = mqttClient;
        _mqttClient.OnMessageReceived += OnMessageReceived;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting MqttService");
        
        await Connect();
        
        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(60));
        try
        {
            while(await timer.WaitForNextTickAsync(stoppingToken))
            {
                // because i can't figure out how to escape the special chars for the heartbeat payload :(
                await _mqttClient.Publish($"{_mqttConfig.BaseTopic}/{HeartbeatTopic}", JsonConvert.SerializeObject(new { service = DateTime.UtcNow }));
                _logger.LogInformation("Sent heartbeat");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Stopping MqttService");
        }
    }

    public override void Dispose()
    {
        _mqttClient.Dispose();
    }

    //public async Task StopAsync(CancellationToken cancellationToken)
    //{
    //    await _mqttClient.Disconnect();
    //    _logger.LogInformation("Disconnected");

    //    await base.StopAsync(cancellationToken);
    //}

    private async void OnMessageReceived(object? sender, MqttMessageReceivedEventArgs e)
    {
        _logger.LogDebug("Received message on topic {Topic} with payload {Payload}", e.Topic, e.Payload);
        if (e.Topic.EndsWith("homeassistant"))
        {
            if (int.TryParse(e.Topic.Split('/')[3], out var result))
            {
                var cmd = e.Payload.Equals("ON", StringComparison.InvariantCultureIgnoreCase) ? Command.Start : Command.Stop; //I know, everything that's not "ON" is "OFF"
                await _messageBus.Publish(new SubdeviceApiCommand() { Cmd = cmd, Id = result });
            }
            else
            {
                _logger.LogWarning("Invalid subdevice id in topic {Topic}", e.Topic);
            }
        }
        else
        {
            var cmd = JsonConvert.DeserializeObject<SubdeviceApiCommand>(e.Payload);
            await _messageBus.Publish(cmd);
        }

    }

    private async Task Connect()
    {
        await _mqttClient.Connect();
        _logger.LogInformation("Connected");

        await _mqttClient.Subscribe($"{_mqttConfig.BaseTopic}/{Helper.BuildMqttSubdeviceCommandTopic()}");
        if (_controllerConfig.HomeAssistantDiscovery)
        {
            await _mqttClient.Subscribe($"{_mqttConfig.BaseTopic}/{Helper.BuildMqttSubdeviceHACommandTopic()}");
        }
        _logger.LogInformation("subscribed to all topics");
    }
}