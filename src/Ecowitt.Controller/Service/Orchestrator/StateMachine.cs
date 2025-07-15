using System.Text.Json;
using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Mapping;
using Ecowitt.Controller.Message;
using Ecowitt.Controller.Message.Config;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Api;
using Ecowitt.Controller.Store;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Ecowitt.Controller.Service.Orchestrator;

public class StateMachine : BackgroundService, IConsumer<SubdeviceApiCommand>, IConsumer<GatewayApiData>, IConsumer<SubdeviceApiAggregate>
{
    private readonly ILogger<StateMachine> _logger;
    private readonly IDeviceStore _deviceStore;
    private readonly EcowittOptions _ecowittOptions;
    private readonly ControllerOptions _controllerOptions;
    private readonly MqttOptions _mqttOptions;
    private readonly IMessageBus _messageBus;

    public StateMachine(ILogger<StateMachine> logger, IDeviceStore deviceStore, IMessageBus messageBus, IOptions<MqttOptions> mqttOptions, IOptions<EcowittOptions> ecowittOptions, IOptions<ControllerOptions> controllerOptions) 
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
        
        var mqttConfig = new MqttConfig
        {
            Host = _mqttOptions.Host,
            Port = _mqttOptions.Port,
            ClientId = _mqttOptions.ClientId,
            BaseTopic = _mqttOptions.BaseTopic,
            ReconnectAttempts = _mqttOptions.ReconnectAttempts,
            HomeAssistantDiscovery = _controllerOptions.HomeAssistantDiscovery,
            Precision = _controllerOptions.Precision,
            PublishingInterval = _controllerOptions.PublishingInterval,
            Units = _controllerOptions.Units
        };
        if (!string.IsNullOrWhiteSpace(_mqttOptions.User))
        {
            mqttConfig.User = _mqttOptions.User;
            mqttConfig.Password = _mqttOptions.Password;
        }
        
        await _messageBus.Publish(mqttConfig);


        // using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(60));
        // try
        // {
        //     while(await timer.WaitForNextTickAsync(stoppingToken))
        //     {
        //         if (_client is { IsConnected: true } && _mqttConfig != null)
        //         {
        //             await Publish($"{_mqttConfig.BaseTopic}/{_mqttConfig.HeartbeatTopic}",
        //                 JsonSerializer.Serialize(new { service = DateTime.UtcNow }));
        //             _logger.LogInformation("Sent heartbeat");
        //         }
        //     }
        // }
        // catch (OperationCanceledException)
        // {
        //     _logger.LogInformation("Stopping MqttService");
        // }
    }

    public async Task OnHandle(SubdeviceApiCommand message)
    {
        _logger.LogInformation($"Received SubdeviceCommand: {message.Cmd} for device {message.Id}");
        
        var gw = _deviceStore.GetGatewayBySubdeviceId(message.Id);
        if(gw == null)
        {
            _logger.LogWarning($"Gateway not found for subdevice {message.Id}");
            return;
        }
        // hey compiler, this can't be null!
        var subdevice = gw.Subdevices.FirstOrDefault(sd => sd.Id == message.Id);

        if (message.Cmd == Command.Start)
        {
            //var val = message.Duration ?? 20;
            //var valType = message.Unit ?? DurationUnit.Minutes;
            // // the magic "maganiFator" :D
            //if(valType == DurationUnit.Liters) val *= 10;
            //var alwaysOn = message.AlwaysOn.HasValue ? 1 : 0;
            //await SendCommand(gw.IpAddress, "quick_run", message.Id, (int)subdevice!.Model, val: val, valType: (int)valType, alwaysOn: alwaysOn);
            
            //default always-on message for now
            await SendCommand(gw.IpAddress, "quick_run", message.Id, (int)subdevice!.Model);

        } else if (message.Cmd == Command.Stop)
        {
            await SendCommand(gw.IpAddress, "quick_stop", message.Id, (int)subdevice!.Model);
        }
        else
        {
            _logger.LogWarning($"Ignoring unsupported command {message.Cmd} for subdevice {message.Id}");
            return;
        }
    }

    public Task OnHandle(GatewayApiData message)
    {
        _logger.LogDebug($"Received ApiData: {message.Model} ({message.PASSKEY}) \n {message.Payload}");
        var updatedGateway = message.Map(_controllerOptions.Units == Units.Metric, _ecowittOptions.CalculateValues);
        updatedGateway.Name = _ecowittOptions.Gateways.FirstOrDefault(g => g.Ip == updatedGateway.IpAddress)?.Name ?? updatedGateway.IpAddress.Replace('.','-');

        var storedGateway = _deviceStore.GetGateway(updatedGateway.IpAddress);
        if(storedGateway == null)
        {
            updatedGateway.DiscoveryUpdate = true;

            foreach (var sensor in updatedGateway.Sensors)
            {
                sensor.DiscoveryUpdate = true;
            }
            if(!_deviceStore.UpsertGateway(updatedGateway)) _logger.LogWarning($"failed to add gateway {updatedGateway.IpAddress} ({updatedGateway.Model}) to the store");
            else { _logger.LogDebug($"gateway updated: {JsonSerializer.Serialize(storedGateway)})"); }
        }
        else
        {
            // no other property should update besides sensors 
            // and i'm stupid, because TS is required for availability :(
            storedGateway.TimestampUtc = updatedGateway.TimestampUtc;
            
            foreach (var sensor in updatedGateway.Sensors)
            {
                var storedSensor = storedGateway.Sensors.FirstOrDefault(s => s.Name == sensor.Name);
                if (storedSensor == null)
                {
                    sensor.DiscoveryUpdate = true;
                    storedGateway.Sensors.Add(sensor);
                }
                else
                {
                    storedSensor.Value = sensor.Value; 
                }
            }
            
            var sensorsToRemove = storedGateway.Sensors.Where(s => updatedGateway.Sensors.All(gs => gs.Name != s.Name)).ToList();
            foreach (var sensor in sensorsToRemove)
            {
                storedGateway.Sensors.Remove(sensor);
            }
            
            if(!_deviceStore.UpsertGateway(storedGateway)) {_logger.LogWarning($"failed to update {storedGateway.IpAddress} ({storedGateway.Model}) in the store");}
            else { _logger.LogDebug($"gateway updated: {JsonSerializer.Serialize(storedGateway)})"); }
        }
        
        return Task.CompletedTask;
    }

    public Task OnHandle(SubdeviceApiAggregate message)
    {
        var ips = message.Subdevices.DistinctBy(sd => sd.GwIp).Select(sd => sd.GwIp);
        foreach (var ip in ips)
        {
            var storedGateway = _deviceStore.GetGateway(ip);
            if (storedGateway == null)
            {
                if (_ecowittOptions.AutoDiscovery)
                {
                    _logger.LogWarning($"Gateway {ip} not found while in autodiscovery mode. Not updating subdevices. (Try turning off autodiscovery)");
                    return Task.CompletedTask;
                }
                
                storedGateway = new Device {IpAddress = ip};
                storedGateway.Name = _ecowittOptions.Gateways.FirstOrDefault(g => g.Ip == storedGateway.IpAddress)?.Name ?? storedGateway.IpAddress.Replace('.','-');
                storedGateway.DiscoveryUpdate = true;
            }

            var subdeviceApiData = message.Subdevices.Where(sd => sd.GwIp == ip);
            foreach (var data in subdeviceApiData)
            {
                var updatedSubDevice = data.Map(_controllerOptions.Units == Units.Metric, _ecowittOptions.CalculateValues);
                var storedSubDevice = storedGateway.Subdevices.FirstOrDefault(gwsd => gwsd.Id == updatedSubDevice.Id);
                if (storedSubDevice == null)
                {
                    updatedSubDevice.DiscoveryUpdate = true;
                    foreach (var sensor in updatedSubDevice.Sensors)
                    {
                        sensor.DiscoveryUpdate = true;
                    }
                    storedGateway.Subdevices.Add(updatedSubDevice);
                    _logger.LogInformation($"subdevice added: {data.Id} ({data.Model})");


                } else {
                    storedSubDevice.TimestampUtc = updatedSubDevice.TimestampUtc;
                    storedSubDevice.Availability = updatedSubDevice.Availability;

                    // no update of other properties
                    if (storedSubDevice.Version != updatedSubDevice.Version || storedSubDevice.Devicename != updatedSubDevice.Devicename || storedSubDevice.Nickname != updatedSubDevice.Nickname)
                    {
                        storedSubDevice.Version = updatedSubDevice.Version;
                        storedSubDevice.Devicename = updatedSubDevice.Devicename;
                        storedSubDevice.Nickname = updatedSubDevice.Nickname;
                        storedSubDevice.DiscoveryUpdate = true;
                    }
                    
                    // update sensors one by one and find out if there are new ones
                    // if there are new ones, mark the subdevice for discovery update
                    foreach (var sensor in updatedSubDevice.Sensors)
                    {
                        var storedSensor = storedSubDevice.Sensors.FirstOrDefault(s => s.Name == sensor.Name);
                        if (storedSensor == null)
                        {
                            sensor.DiscoveryUpdate = true;
                            storedSubDevice.Sensors.Add(sensor);
                        }
                        else
                        {
                            storedSensor.Value = sensor.Value; 
                        }
                    }
                    
                    // remove sensors that are not in the update
                    var sensorsToRemove = storedSubDevice.Sensors.Where(s => updatedSubDevice.Sensors.All(us => us.Name != s.Name)).ToList();
                    foreach (var sensor in sensorsToRemove)
                    {
                        storedSubDevice.Sensors.Remove(sensor);
                    }
                
                    _logger.LogInformation($"subdevice updated: {data.Id} ({data.Model})");
                }
            }
            
            _deviceStore.UpsertGateway(storedGateway);
        }
        
        return Task.CompletedTask;
    }
    
    // public async Task<bool> SendCommand(string ipAddress, string cmd, int id, int model, int val = 0, int valType = 0, int onType = 0, int offType = 0, int alwaysOn = 1, int onTime = 0, int offTime = 0)
    // {
    //     var client = _httpClientFactory.CreateClient("ecowitt-client");
    //     client.BaseAddress = new Uri($"http://{ipAddress}");
    //
    //     var username = _ecowittOptions.Gateways.FirstOrDefault(gw => gw.Ip == ipAddress)?.Username;
    //     var password = _ecowittOptions.Gateways.FirstOrDefault(gw => gw.Ip == ipAddress)?.Password;
    //     if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
    //     {
    //         //TODO: authentication header, need to test
    //         //client.DefaultRequestHeaders.Add();
    //     }
    //     
    //     // [{"on_type":0,"off_type":0,"always_on":0,"on_time":0,"off_time":0,"val_type":1,"val":20,"cmd":"quick_run","id":12345,"model":1}]}    
    //     dynamic payload;
    //     switch (cmd)
    //     {
    //         case "quick_run":
    //             payload = new { command = new[] { new { cmd, id, model, val, val_type = valType, on_type = onType, off_type = offType, always_on = alwaysOn, on_time = onTime, off_time = offTime } } };
    //             break;
    //         case "quick_stop":
    //             payload = new { command = new[] { new { cmd, id, model } } };
    //             break;
    //         default:
    //             _logger.LogWarning($"Unsupported command type {cmd}. Not sending command to {ipAddress} for subdevice {id}");
    //             return false;
    //     }
    //     
    //     var sContent = new StringContent(JsonSerializer.Serialize(payload));
    //     var response = await client.PostAsync("parse_quick_cmd_iot", sContent);
    //
    //     if (response.IsSuccessStatusCode) return true;
    //     else {
    //         _logger.LogWarning($"Could not send command to {ipAddress} for subdevice {id}");
    //         return false;
    //     }
    // }

}