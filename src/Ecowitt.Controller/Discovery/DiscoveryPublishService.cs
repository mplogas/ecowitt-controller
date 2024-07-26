using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Discovery.Model;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Mqtt;
using Ecowitt.Controller.Store;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Ecowitt.Controller.Discovery;

public class DiscoveryPublishService : BackgroundService
{
    private readonly ILogger<DiscoveryPublishService> _logger;
    private readonly MqttOptions _mqttOptions;
    private readonly IDeviceStore _store;
    private readonly IMqttClient _mqttClient;
    private readonly ControllerOptions _controllerOptions;

    private readonly Origin _origin;

    public DiscoveryPublishService(IOptions<MqttOptions> mqttOptions, IOptions<ControllerOptions> controllerOption,
        ILogger<DiscoveryPublishService> logger, IMqttClient mqttClient,
        IDeviceStore deviceStore)
    {
        _logger = logger;
        _mqttOptions = mqttOptions.Value;
        _controllerOptions = controllerOption.Value;
        _store = deviceStore;
        _mqttClient = mqttClient;
        _origin = new Origin()
            { Name = "Ecowitt Controller", Sw = "0.1", Url = @"https://github.com/mplogas/ecowitt-controller" };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_controllerOptions.HomeAssistantDiscovery)
        {
            _logger.LogInformation("Home Assistant discovery is disabled");
            return;
        }

        _logger.LogInformation("Starting DiscoveryPublishService");

        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(_controllerOptions.PublishingInterval));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                foreach (var gw in _store.GetGatewaysShort().Select(gwKvp => _store.GetGateway(gwKvp.Key)).OfType<Gateway>())
                {
                    if (gw.DiscoveryUpdate)
                    {
                        await PublishGatewayDiscovery(gw);
                    }

                    foreach (var sensor in gw.Sensors.Where(sensor => sensor.DiscoveryUpdate))
                    {
                        await PublishSensorDiscovery(gw, sensor);
                    }

                    foreach (var subdevice in gw.Subdevices)
                    {
                        if (subdevice.DiscoveryUpdate)
                        {
                            await PublishSubdeviceDiscovery(gw, subdevice);
                        }

                        foreach (var sensor in subdevice.Sensors.Where(sensor => sensor.DiscoveryUpdate))
                        {
                            await PublishSensorDiscovery(subdevice, sensor);
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Stopping MqttService");
        }
    }



    private async Task PublishGatewayDiscovery(Gateway gw)
    {
        var device = DiscoveryBuilder.BuildDevice(gw.Name, gw.Model ?? "unknown", "Ecowitt", gw.StationType??"unknown", gw.Runtime.ToString()??"unknown");
        var availability = DiscoveryBuilder.BuildAvailability($"{_mqttOptions.BaseTopic}/{gw.Name}", "online", "offline", "{{value.state}}");

        var id = $"ecowitt-controller_{gw.Name}_availability_state";
        var topic = $"{_mqttOptions.BaseTopic}/{gw.Name}";

        var config = DiscoveryBuilder.BuildConfig(device, _origin, "Availability", id, id, topic, string.Empty, string.Empty, null, null, null, new List<Availability> { availability });

        await PublishMessage($"sensor/{gw.Name}", config);

    }
    private async Task PublishSubdeviceDiscovery(Gateway gw, Ecowitt.Controller.Model.Subdevice subdevice)
    {
        throw new NotImplementedException();
    }

    private async Task PublishSensorDiscovery(Gateway gw, ISensor sensor)
    {
        throw new NotImplementedException();
    }

    private async Task PublishSensorDiscovery(Ecowitt.Controller.Model.Subdevice subdevice, ISensor sensor)
    {
        throw new NotImplementedException();
    }

    private async Task PublishMessage(string topic, Config config)
    {
        if (!await _mqttClient.Publish($"homeassistant/{topic}/config",
                JsonConvert.SerializeObject(config)))
            _logger.LogWarning($"Failed to publish message to topic homeassistant/{topic}/config. Is the client connected?");
    }
}
