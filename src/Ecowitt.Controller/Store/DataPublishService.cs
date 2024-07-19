using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Mqtt;
using Microsoft.Extensions.Options;
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
                    dynamic jsonPayload = new
                    {
                        ip = gw.IpAddress,
                        name = gw.Name,
                        subdevices = gw.Subdevices.Select(sd => new
                        {
                            id = sd.Id,
                            model = sd.Model,
                            devicename = sd.Devicename,
                            nickname = sd.Nickname,
                            availability = sd.Availability,
                            ver = sd.Version,
                            timestampUtc = sd.TimestampUtc,
                            payload = sd.Sensors.Select(s => new
                            {
                                name = s.Name,
                                value = s.Value,
                                unit = s.UnitOfMeasurement,
                                type = s.SensorType.ToString()
                                }).ToArray()
                        })
                    };
                    if (!await _mqttClient.Publish($"{_mqttOptions.BaseTopic}/{gw.Name}",
                            JsonConvert.SerializeObject(jsonPayload, Formatting.None,
                                new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }))) 
                        _logger.LogWarning($"Failed to publish {gw.IpAddress}. Is the client connected?");
                }

                //_store.GetGatewaysShort();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Stopping MqttService");
        }
    }
}