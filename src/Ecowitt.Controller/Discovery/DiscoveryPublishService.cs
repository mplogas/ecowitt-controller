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
        _origin = DiscoveryBuilder.BuildOrigin();
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
                        gw.DiscoveryUpdate = false;
                        await PublishGatewayDiscovery(gw);
                    }

                    foreach (var sensor in gw.Sensors.Where(sensor => sensor.DiscoveryUpdate))
                    {
                        sensor.DiscoveryUpdate = false;
                        await PublishSensorDiscovery(gw, sensor);
                    }

                    foreach (var subdevice in gw.Subdevices)
                    {
                        if (subdevice.DiscoveryUpdate)
                        {
                            subdevice.DiscoveryUpdate = false;
                            await PublishSubdeviceDiscovery(gw, subdevice);
                        }

                        foreach (var sensor in subdevice.Sensors.Where(s => s.DiscoveryUpdate))
                        {
                            sensor.DiscoveryUpdate = false;
                            await PublishSensorDiscovery(gw, subdevice, sensor);
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
        var device = gw.Model == null ? DiscoveryBuilder.BuildDevice(gw.Name) : DiscoveryBuilder.BuildDevice(gw.Name, gw.Model, "Ecowitt", gw.Model, gw.StationType??"unknown");
        var id = DiscoveryBuilder.BuildIdentifier(gw.Name, "availability");
        //var statetopic = $"{_mqttOptions.BaseTopic}/{Helper.BuildMqttGatewayTopic(gw.Name)}";
        var availabilityTopic = $"{_mqttOptions.BaseTopic}/{Helper.BuildMqttGatewayTopic(gw.Name)}/availability";

        var config = DiscoveryBuilder.BuildGatewayConfig(device, _origin, "Availability", id, availabilityTopic, availabilityTopic);

        await PublishMessage(Helper.Sanitize($"sensor/{gw.Name}"), config);

    }
    private async Task PublishSubdeviceDiscovery(Gateway gw, Ecowitt.Controller.Model.Subdevice subdevice)
    {
        var device = DiscoveryBuilder.BuildDevice(subdevice.Nickname, subdevice.Model.ToString(), "Ecowitt", subdevice.Model.ToString(), subdevice.Version.ToString(), DiscoveryBuilder.BuildIdentifier(gw.Name));
        var id = DiscoveryBuilder.BuildIdentifier(subdevice.Nickname, "availability");
        //var statetopic = $"{_mqttOptions.BaseTopic}/{Helper.BuildMqttSubdeviceTopic(gw.Name, subdevice.Id.ToString())}";
        var availabilityTopic = $"{_mqttOptions.BaseTopic}/{Helper.BuildMqttSubdeviceTopic(gw.Name, subdevice.Id.ToString())}/availability";

        var config = DiscoveryBuilder.BuildGatewayConfig(device, _origin, "Availability", id, availabilityTopic, availabilityTopic);

        await PublishMessage(Helper.Sanitize($"sensor/{subdevice.Nickname}"), config);
    }

    private async Task PublishSensorDiscovery(Gateway gw, ISensor sensor)
    {
        var device = gw.Model == null ? DiscoveryBuilder.BuildDevice(gw.Name) : DiscoveryBuilder.BuildDevice(gw.Name, gw.Model, "Ecowitt", gw.Model, gw.StationType ?? "unknown");
        var statetopic = $"{_mqttOptions.BaseTopic}/{Helper.BuildMqttGatewaySensorTopic(gw.Name, sensor.Name, sensor.SensorType.ToString())}";
        var availabilityTopic = $"{_mqttOptions.BaseTopic}/{Helper.BuildMqttGatewayTopic(gw.Name)}/availability";
        await PublishSensorDiscovery(device, sensor, statetopic, availabilityTopic);
    }

    private async Task PublishSensorDiscovery(Gateway gw, Ecowitt.Controller.Model.Subdevice subdevice, ISensor sensor)
    {
        var device = DiscoveryBuilder.BuildDevice(subdevice.Nickname, subdevice.Model.ToString(), "Ecowitt", subdevice.Model.ToString(), subdevice.Version.ToString(), DiscoveryBuilder.BuildIdentifier(gw.Name));
        var statetopic = $"{_mqttOptions.BaseTopic}/{Helper.BuildMqttSubdeviceSensorTopic(gw.Name, subdevice.Id.ToString(), sensor.Name, sensor.SensorType.ToString())}";
        var availabilityTopic = $"{_mqttOptions.BaseTopic}/{Helper.BuildMqttSubdeviceTopic(gw.Name, subdevice.Id.ToString())}/availability";
        await PublishSensorDiscovery(device, sensor, statetopic, availabilityTopic);
    }

    private async Task PublishSensorDiscovery(Device device, ISensor sensor, string statetopic, string availabilityTopic)
    {
        var id = DiscoveryBuilder.BuildIdentifier($"{device.Name}_{sensor.Name}", sensor.SensorType.ToString());

        var category = DiscoveryBuilder.BuildDeviceCategory(sensor.SensorType);

        var config = DiscoveryBuilder.BuildSensorConfig(device, _origin, sensor.Name, id, category, statetopic, "{{ value_json.value }}", sensor.UnitOfMeasurement, string.Empty);

        var sensorClassTopic = BuildSensorClassTopic(sensor.SensorClass); 

        await PublishMessage(Helper.Sanitize($"{sensorClassTopic}/{device.Name}_{sensor.Name}"), config);
    }

    private async Task PublishMessage(string topic, Config config)
    {
        topic = $"homeassistant/{topic}/config";

        if(config.DeviceClass != null && config.DeviceClass.Equals("none", StringComparison.InvariantCultureIgnoreCase)) config.DeviceClass = null;
        var payload = JsonConvert.SerializeObject(config,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        _logger.LogDebug($"Topic: {topic}");
        _logger.LogDebug($"Payload: {payload}");

        if (!await _mqttClient.Publish(topic, payload))
            _logger.LogWarning($"Failed to publish message to topic {topic}. Is the client connected?");
    }

    private string BuildSensorClassTopic(SensorClass sc)
    {
        return sc switch
        {
            SensorClass.BinarySensor => "binary_sensor",
            _ => "sensor"
        };
    }
}
