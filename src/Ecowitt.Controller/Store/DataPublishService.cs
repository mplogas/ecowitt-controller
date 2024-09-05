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

                    // note: HA Discovery seems to break for mqtt topics with spaces in them

                    var payload = BuildGatewayPayload(gw);
                    
                    await PublishMessage(Helper.BuildMqttGatewayTopic(gw.Name), payload);
                    await PublishAvailabilityMessage(Helper.BuildMqttGatewayTopic(gw.Name), gw.TimestampUtc);

                    foreach (var sensor in gw.Sensors)
                    {
                        payload = BuildSensorPayload(sensor);
                        var topic = sensor.SensorCategory == SensorCategory.Diagnostic ? Helper.BuildMqttGatewayDiagnosticTopic(gw.Name, sensor.Name) : Helper.BuildMqttGatewaySensorTopic(gw.Name, sensor.Name);
                        await PublishMessage(topic, payload);

                    }

                    if (gw.Subdevices.Count == 0) continue;
                    foreach (var subdevice in gw.Subdevices)
                    {
                        payload = BuildSubdevicePayload(subdevice);
                        await PublishMessage(Helper.BuildMqttSubdeviceTopic(gw.Name, subdevice.Id.ToString()), payload);
                        await PublishAvailabilityMessage(Helper.BuildMqttSubdeviceTopic(gw.Name, subdevice.Id.ToString()), subdevice.TimestampUtc);

                        foreach (var sensor in subdevice.Sensors)
                        {
                            payload = BuildSensorPayload(sensor);
                            var topic = sensor.SensorCategory == SensorCategory.Diagnostic ? Helper.BuildMqttSubdeviceDiagnosticTopic(gw.Name, subdevice.Id.ToString(), sensor.Name) : Helper.BuildMqttSubdeviceSensorTopic(gw.Name, subdevice.Id.ToString(), sensor.Name);
                            await PublishMessage(topic, payload);
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

    private dynamic BuildSensorPayload(ISensor s)
    {
        _logger.LogDebug($"Sensor {s.Name} datatype: {s.DataType}");
        return new
        {
            name = s.Name,
            value = s.DataType == typeof(double?) ? Math.Round(Convert.ToDouble(s.Value), _controllerOptions.Precision) : s.Value,
            unit = !string.IsNullOrWhiteSpace(s.UnitOfMeasurement) ? s.UnitOfMeasurement : null,
            type = s.SensorType != SensorType.None ? s.SensorType.ToString() : null
        };
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
            state = (DateTime.UtcNow - gw.TimestampUtc).TotalSeconds < _controllerOptions.PublishingInterval * 3 ? "online" : "offline",
            freq = gw.Freq
        };
    }

    private async Task PublishMessage(string topic, dynamic payload)
    {
        if (!await _mqttClient.Publish($"{_mqttOptions.BaseTopic}/{topic}",
                JsonConvert.SerializeObject(payload,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }))) 
            _logger.LogWarning($"Failed to publish message to topic {_mqttOptions.BaseTopic}/{topic}. Is the client connected?");
    }

    private async Task PublishAvailabilityMessage(string topic, DateTime timestamp)
    {
        var available = DateTime.UtcNow.Subtract(timestamp) < TimeSpan.FromSeconds(300) ? "online" : "offline";

        if (!await _mqttClient.Publish($"{_mqttOptions.BaseTopic}/{topic}/availability", available))
            _logger.LogWarning($"Failed to publish message to topic {_mqttOptions.BaseTopic}/{topic}. Is the client connected?");
    }
}