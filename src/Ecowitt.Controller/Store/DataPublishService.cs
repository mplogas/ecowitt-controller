using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Mqtt;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace Ecowitt.Controller.Store;

public class DataPublishService : BackgroundService
{
    private readonly ILogger<DataPublishService> _logger;
    private readonly MqttOptions _mqttOptions;
    private readonly IDeviceStore _store;
    private readonly IMqttClient _mqttClient;
    private readonly ControllerOptions _controllerOptions;

    public DataPublishService(IOptions<MqttOptions> mqttOptions, IOptions<ControllerOptions> controllerOption, ILogger<DataPublishService> logger, IMqttClient mqttClient,
        IDeviceStore deviceStore)
    {
        _logger = logger;
        _mqttOptions = mqttOptions.Value;
        _controllerOptions = controllerOption.Value;
        _store = deviceStore;
        _mqttClient = mqttClient;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting MqttPublishService");
        
        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(_controllerOptions.PublishingInterval));
        try
        {
            while(await timer.WaitForNextTickAsync(stoppingToken))
            {
                foreach (var gwKvp in _store.GetGatewaysShort())
                {
                    var gw = _store.GetGateway(gwKvp.Key);
                    if(gw == null) continue;
                    
                    // to sent messages according to ideas outlines in mqtt.md the following approach could work
                    // 1. send gateway state & hw_info to base/<gateway_id>/ as json payload
                    // 2. send gateway sensors to base/<gateway_id>/sensors/<sensorid> as json payload
                    // 3. send subdevice state & hw_info to base/<gateway_id>/<subdevice_id> as json payload
                    // 4. send subdevice sensors to base/<gateway_id>/<subdevice_id>/sensors/<sensorid> as json payload

                    dynamic payload = BuildGatewayPayload(gw);
                    var json = JsonConvert.SerializeObject(payload);
                    
                    await PublishMessage(gw.Name, payload);
                    await PublishAvailabilityMessage($"{gw.Name}", gw.TimestampUtc);

                    payload = BuildSensorPayloads(gw.Sensors);
                    foreach (var sensor in payload)
                    {
                        await PublishMessage($"{gw.Name}/sensors/{sensor.name}/{sensor.type}", sensor);
                    }

                    if (gw.Subdevices.Count == 0) continue;
                    foreach (var subdevice in gw.Subdevices)
                    {
                        payload = BuildSubdevicePayload(subdevice);
                        await PublishMessage($"{gw.Name}/subdevices/{subdevice.Id}", payload);
                        await PublishAvailabilityMessage($"{gw.Name}/subdevices/{subdevice.Id}", subdevice.TimestampUtc);

                        var sensorPayload = BuildSensorPayloads(subdevice.Sensors);
                        foreach (var sensor in sensorPayload)
                        {
                            await PublishMessage($"{gw.Name}/subdevices/{subdevice.Id}/sensors/{sensor.name}/{sensor.type}", sensor);
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

    private dynamic BuildSensorPayloads(List<ISensor> sensors)
    {
        return sensors.Select(s => new
        {
            name = s.Name,
            value = s.DataType == typeof(double) ? Math.Round(Convert.ToDouble(s.Value), _controllerOptions.Precision) : s.Value,
            unit = s.UnitOfMeasurement,
            type = s.SensorType.ToString()
        }).ToList();
    }

    private dynamic BuildGatewayPayload(Gateway gw)
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
            state = (DateTime.UtcNow - gw.TimestampUtc).TotalSeconds < 60 ? "online" : "offline",
            freq = gw.Freq
        };
    }

    private async Task PublishMessage(string topic, dynamic payload)
    {
        if (!await _mqttClient.Publish($"{_mqttOptions.BaseTopic}/{topic}",
                JsonConvert.SerializeObject(payload))) 
            _logger.LogWarning($"Failed to publish message to topic {_mqttOptions.BaseTopic}/{topic}. Is the client connected?");
    }

    private async Task PublishAvailabilityMessage(string topic, DateTime timestamp)
    {
        var available = DateTime.UtcNow.Subtract(new TimeSpan(0,2,0)) < timestamp ? "offline" : "online";

        if (!await _mqttClient.Publish($"{_mqttOptions.BaseTopic}/{topic}/availability", available))
            _logger.LogWarning($"Failed to publish message to topic {_mqttOptions.BaseTopic}/{topic}. Is the client connected?");
    }
}