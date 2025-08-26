using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Api;
using System.Text.Json;
using SlimMessageBus;
using Ecowitt.Controller.Model.Message.Data;
using Ecowitt.Controller.Model.Message.Event;
using Ecowitt.Controller.Model.Mapping;
using Ecowitt.Controller.Model.Configuration;

namespace Ecowitt.Controller.Service.Orchestrator
{
    public partial class StateMachine : IConsumer<HttpServiceEvent>
    {
        public async Task OnHandle(SubdeviceApiCommand message)
        {
            _logger.LogInformation($"Received SubdeviceCommand: {message.Cmd} for device {message.Id}");

            var gw = _deviceStore.GetGatewayBySubdeviceId(message.Id);
            if (gw == null)
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
                //await SendCommand(gw.IpAddress, "quick_run", message.Id, (int)subdevice!.Model);

            }
            else if (message.Cmd == Command.Stop)
            {
                //await SendCommand(gw.IpAddress, "quick_stop", message.Id, (int)subdevice!.Model);
            }
            else
            {
                _logger.LogWarning($"Ignoring unsupported command {message.Cmd} for subdevice {message.Id}");
                return;
            }
        }

        public async Task OnHandle(GatewayApiData message)
        {
            _logger.LogDebug($"Received ApiData: {message.Model} ({message.PASSKEY}) \n {message.Payload}");
            var updatedGateway = message.Map(_controllerOptions.Units == Units.Metric, _ecowittOptions.CalculateValues);
            updatedGateway.Name = _ecowittOptions.Gateways.FirstOrDefault(g => g.Ip == updatedGateway.IpAddress)?.Name ?? updatedGateway.IpAddress.Replace('.', '-');

            var storedGateway = _deviceStore.GetGateway(updatedGateway.IpAddress);
            if (storedGateway == null)
            {
                updatedGateway.DiscoveryUpdate = true;

                foreach (var sensor in updatedGateway.Sensors)
                {
                    sensor.DiscoveryUpdate = true;
                }
                if (_deviceStore.UpsertGateway(updatedGateway)) 
                {
                    _logger.LogDebug($"gateway added: {JsonSerializer.Serialize(storedGateway)})");
                    await EmitGatewayFull(updatedGateway);
                }
                else _logger.LogWarning($"failed to add gateway {updatedGateway.IpAddress} ({updatedGateway.Model}) to the store");
            }
            else
            {
                // no other property should update besides sensors - it seems fw isn't reported by the GW 
                storedGateway.TimestampUtc = updatedGateway.TimestampUtc;
                var changedSensors = new List<ISensor>();

                foreach (var sensor in updatedGateway.Sensors)
                {
                    var storedSensor = storedGateway.Sensors.FirstOrDefault(s => s.Name == sensor.Name);
                    if (storedSensor == null)
                    {
                        sensor.DiscoveryUpdate = true;
                        storedGateway.Sensors.Add(sensor);
                        changedSensors.Add(sensor);
                    }
                    else if (DeNoiserHelper.HasSignificantChange(storedSensor, sensor.Value))
                    {
                        storedSensor.Value = sensor.Value;
                        changedSensors.Add(storedSensor);
                    }
                }

                await EmitGatewayChanged(changedSensors, storedGateway.IpAddress);

                var sensorsToRemove = storedGateway.Sensors.Where(s => updatedGateway.Sensors.All(gs => gs.Name != s.Name)).ToList();
                foreach (var sensor in sensorsToRemove)
                {
                    storedGateway.Sensors.Remove(sensor);
                }

                // TODO: emit removed gateway sensors to mqtt (maybe... is it really needed?)

                if (!_deviceStore.UpsertGateway(storedGateway)) { _logger.LogWarning($"failed to update {storedGateway.IpAddress} ({storedGateway.Model}) in the store"); }
                else { _logger.LogDebug($"gateway updated: {JsonSerializer.Serialize(storedGateway)})"); }
            }

            LogStorageState();
        }

        public async Task OnHandle(SubdeviceApiAggregate message)
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
                        return;
                    }

                    storedGateway = new Device { IpAddress = ip };
                    storedGateway.Name = _ecowittOptions.Gateways.FirstOrDefault(g => g.Ip == storedGateway.IpAddress)?.Name ?? storedGateway.IpAddress.Replace('.', '-');
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

                        await EmitSubdeviceFull(updatedSubDevice);
                    }
                    else
                    {
                        storedSubDevice.TimestampUtc = updatedSubDevice.TimestampUtc;
                        storedSubDevice.Availability = updatedSubDevice.Availability;

                        // no update of other properties
                        if (storedSubDevice.Version != updatedSubDevice.Version || storedSubDevice.Devicename != updatedSubDevice.Devicename || storedSubDevice.Nickname != updatedSubDevice.Nickname)
                        {
                            storedSubDevice.Version = updatedSubDevice.Version;
                            storedSubDevice.Devicename = updatedSubDevice.Devicename;
                            storedSubDevice.Nickname = updatedSubDevice.Nickname;
                            storedSubDevice.DiscoveryUpdate = true;

                            await EmitSubdeviceFull(storedSubDevice);
                        }

                        // update sensors one by one and find out if there are new ones
                        // if there are new ones, mark the subdevice for discovery update
                        var changedSensors = new List<ISensor>();
                        foreach (var sensor in updatedSubDevice.Sensors)
                        {
                            var storedSensor = storedSubDevice.Sensors.FirstOrDefault(s => s.Name == sensor.Name);
                            if (storedSensor == null)
                            {
                                sensor.DiscoveryUpdate = true;
                                storedSubDevice.Sensors.Add(sensor);
                                changedSensors.Add(sensor);
                            }
                            else if (DeNoiserHelper.HasSignificantChange(storedSensor, sensor.Value))
                            {
                                storedSensor.Value = sensor.Value;
                                changedSensors.Add(storedSensor);
                            }
                        }

                        await EmitSubdeviceChanged(changedSensors, storedGateway.IpAddress, storedSubDevice.Id);

                        // remove sensors that are not in the update
                        var sensorsToRemove = storedSubDevice.Sensors.Where(s => updatedSubDevice.Sensors.All(us => us.Name != s.Name)).ToList();
                        foreach (var sensor in sensorsToRemove)
                        {
                            storedSubDevice.Sensors.Remove(sensor);
                        }

                        // TODO: emit removed subdevice sensors to mqtt (maybe... is it really needed?)

                        _logger.LogInformation($"subdevice updated: {data.Id} ({data.Model})");
                    }
                }

                _deviceStore.UpsertGateway(storedGateway);
            }

            LogStorageState();
        }


        public Task OnHandle(HttpServiceEvent message)
        {
            switch (message.EventType)
            {
                case HttpServiceEventType.Started:
                    _logger.LogInformation("HTTP Service started");
                    _lastHttpServiceState = HttpServiceEventType.Started;
                    break;
                case HttpServiceEventType.Stopped:
                    _logger.LogWarning("HTTP Service stopped");
                    _lastHttpServiceState = HttpServiceEventType.Stopped;
                    break;
                case HttpServiceEventType.Error:
                    _logger.LogError($"HTTP Service error: {message.Message}");
                    _lastHttpServiceState = HttpServiceEventType.Error;
                    break;
                case HttpServiceEventType.Unknown:
                default:
                    _logger.LogWarning($"Unknown HTTP Service event: {message.EventType}");
                    _lastHttpServiceState = HttpServiceEventType.Unknown;
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
