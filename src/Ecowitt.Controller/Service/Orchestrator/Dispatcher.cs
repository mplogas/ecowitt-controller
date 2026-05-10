using System.Text.Json;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Api;
using Ecowitt.Controller.Model.Configuration;
using Ecowitt.Controller.Model.Message.Config;
using Ecowitt.Controller.Model.Message.Data;
using Ecowitt.Controller.Model.Message.Event;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Ecowitt.Controller.Service.Orchestrator;

public partial class Dispatcher : BackgroundService, IConsumer<MqttServiceEvent>, IConsumer<MqttConnectionEvent>, IConsumer<HomeAssistantStatusEvent>, IConsumer<SubdeviceApiCommand>, IConsumer<GatewayApiData>, IConsumer<SubdeviceApiAggregate>
{
    private readonly ILogger<Dispatcher> _logger;
    private readonly IDeviceStore _deviceStore;
    private readonly EcowittOptions _ecowittOptions;
    private readonly ControllerOptions _controllerOptions;
    private readonly MqttOptions _mqttOptions;
    private readonly IMessageBus _messageBus;

    public Dispatcher(ILogger<Dispatcher> logger, IDeviceStore deviceStore, IMessageBus messageBus, IOptions<MqttOptions> mqttOptions, IOptions<EcowittOptions> ecowittOptions, IOptions<ControllerOptions> controllerOptions)
    {
        _logger = logger;
        _deviceStore = deviceStore;
        _mqttOptions = mqttOptions.Value;
        _ecowittOptions = ecowittOptions.Value;
        _controllerOptions = controllerOptions.Value;
        _messageBus = messageBus;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Orchestrator");

        _logger.LogInformation("Emitting initial configuration");
        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        await EmitMqttConfig();
        if(_ecowittOptions.Gateways.Count > 0) await EmitHttpConfig();


    }

    private async Task EmitHomeAssistantDiscovery(Device device)
    {
        var discoveryEvent = new HomeAssistantDiscoveryEvent(device);
        await _messageBus.Publish(discoveryEvent);
    }

    private async Task EmitDiscoveryRemoval(string deviceName, List<ISensor> sensors)
    {
        if (sensors.Count == 0) return;
        await _messageBus.Publish(new DiscoveryRemovalEvent { DeviceName = deviceName, Sensors = sensors });
    }

    private async Task EmitHttpConfig()
    {
        var httpConfig = new HttpConfig
        {
            Hosts = _ecowittOptions.Gateways.Where(gw => gw.Subdevices).Select(gw => new HttpHost(gw.Ip, gw.Username, gw.Password)).ToList(),
            PollingInterval = _ecowittOptions.PollingInterval
        };

        await _messageBus.Publish(httpConfig);
    }

    private async Task EmitHttpConfig(List<HttpHost> hosts)
    {
        var httpConfig = new HttpConfig
        {
            Hosts = hosts,
            PollingInterval = _ecowittOptions.PollingInterval
        };

        await _messageBus.Publish(httpConfig);
    }

    private async Task EmitMqttConfig()
    {
        var mqttConfig = new MqttConfig
        {
            Host = _mqttOptions.Host,
            Port = _mqttOptions.Port,
            ClientId = _mqttOptions.ClientId,
            BaseTopic = _mqttOptions.BaseTopic,
            ReconnectAttempts = _mqttOptions.ReconnectAttempts,
            UseMqtt311 = _mqttOptions.UseMqtt311,
            HomeAssistantDiscovery = _controllerOptions.HomeAssistantDiscovery,
            Precision = _controllerOptions.Precision,
            Units = _controllerOptions.Units
        };
        if (!string.IsNullOrWhiteSpace(_mqttOptions.User))
        {
            mqttConfig.User = _mqttOptions.User;
            mqttConfig.Password = _mqttOptions.Password;
        }

        await _messageBus.Publish(mqttConfig);
    }

    private void LogStorageState()
    {
        foreach (var gw in _deviceStore.GetGatewaysShort())
        {
            _logger.LogInformation("Gateway {GwKey} - {GwValue}", gw.Key, gw.Value);
            var gateway = _deviceStore.GetGateway(gw.Key);
            _logger.LogDebug("Storage dump: \n {Serialize}", JsonSerializer.Serialize(gateway));
        }
    }

    private async Task EmitGatewayFull(Device device)
    {
        await _messageBus.Publish(new DeviceDataFull
        {
            Device = device,
            GatewayId = device.IpAddress,
            GatewayName = device.Name,
            Timestamp = device.TimestampUtc
        });
    }

    private async Task EmitSubdeviceFull(Subdevice subdevice)
    {
        var gateway = _deviceStore.GetGateway(subdevice.GwIp);
        var gatewayName = gateway != null ? gateway.Name : subdevice.GwIp;

        await _messageBus.Publish(new SubdeviceDataFull
        {
            Subdevice = subdevice,
            GatewayId = subdevice.GwIp,
            GatewayName = gatewayName,
            SubdeviceId = subdevice.Id,
            Timestamp = subdevice.TimestampUtc
        });
    }

    private async Task EmitGatewayChanged(List<ISensor> sensorsChanged, string gatewayId, string gatewayName)
    {
        await _messageBus.Publish(new DeviceData()
        {
            ChangedSensors = sensorsChanged,
            GatewayId = gatewayId,
            GatewayName = gatewayName,
            Timestamp = DateTime.UtcNow
        });
    }

    private async Task EmitSubdeviceChanged(List<ISensor> sensorsChanged, string gatewayId, int subdeviceId)
    {
        var gateway = _deviceStore.GetGatewayBySubdeviceId(subdeviceId);
        var gatewayName = gateway != null ? gateway.Name : gatewayId;

        await _messageBus.Publish(new SubdeviceData()
        {
            ChangedSensors = sensorsChanged,
            GatewayId = gatewayId,
            SubdeviceId = subdeviceId,
            GatewayName = gatewayName,
            Timestamp = DateTime.UtcNow
        });
    }

}