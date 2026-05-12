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
    public partial class Dispatcher : IConsumer<HttpServiceEvent>
    {
        public async Task OnHandle(SubdeviceApiCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received SubdeviceCommand: {MessageCmd} for device {MessageId}", message.Cmd, message.Id);

            var gw = _deviceStore.GetGatewayBySubdeviceId(message.Id);
            if (gw == null)
            {
                _logger.LogWarning("Gateway not found for subdevice {MessageId}", message.Id);
                return;
            }

            var subdevice = gw.Subdevices.FirstOrDefault(sd => sd.Id == message.Id);
            if (subdevice == null)
            {
                _logger.LogWarning("Subdevice {MessageId} not found in gateway {GwIp}", message.Id, gw.IpAddress);
                return;
            }

            if (message.Cmd is not (Command.Start or Command.Stop))
            {
                _logger.LogWarning("Ignoring unsupported command {MessageCmd} for subdevice {MessageId}", message.Cmd, message.Id);
                return;
            }

            // HA sends bare ON/OFF without duration — default to always-on
            if (message.Cmd == Command.Start && !message.Duration.HasValue)
            {
                message.AlwaysOn = true;
            }

            await _messageBus.Publish(new SubdeviceCommandDispatch
            {
                GatewayIp = gw.IpAddress,
                Command = message,
                Model = subdevice.Model
            });
        }

        public async Task OnHandle(GatewayApiData message, CancellationToken cancellationToken)
        {
            // Drop push payloads from Poll-mode gateways — they get their data via livedata poll instead.
            var configuredGateway = _ecowittOptions.Gateways.FirstOrDefault(g => g.Ip == message.IpAddress);
            if (configuredGateway?.IngestMode == IngestMode.Poll)
            {
                _logger.LogDebug("Dropping push payload from Poll-mode gateway {GatewayIp}", message.IpAddress);
                return;
            }

            _logger.LogDebug("Received ApiData: {MessageModel} ({MessagePasskey}) \n {MessagePayload}", message.Model, message.PASSKEY, message.Payload);
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
                    _logger.LogDebug("gateway added: {Serialize})", JsonSerializer.Serialize(storedGateway));
                    await EmitHomeAssistantDiscovery(updatedGateway);
                    await EmitGatewayFull(updatedGateway);
                }
                else _logger.LogWarning("failed to add gateway {UpdatedGatewayIpAddress} ({UpdatedGatewayModel}) to the store", updatedGateway.IpAddress, updatedGateway.Model);
            }
            else
            {
                // no other property should update besides sensors - it seems fw isn't reported by the GW 
                storedGateway.TimestampUtc = updatedGateway.TimestampUtc;
                var changedSensors = new List<ISensor>();
                var emitDiscovery = false;
                foreach (var sensor in updatedGateway.Sensors)
                {
                    var storedSensor = storedGateway.Sensors.FirstOrDefault(s => s.Name == sensor.Name);
                    if (storedSensor == null)
                    {
                        sensor.DiscoveryUpdate = true;
                        storedGateway.Sensors.Add(sensor);
                        changedSensors.Add(sensor);
                        emitDiscovery = true;
                    }
                    else if (DeNoiserHelper.HasSignificantChange(storedSensor, sensor.Value))
                    {
                        storedSensor.Value = sensor.Value;
                        changedSensors.Add(storedSensor);
                    }
                }

                if(changedSensors.Count > 0) await EmitGatewayChanged(changedSensors, storedGateway.IpAddress, storedGateway.Name);
                else _logger.LogInformation("no changes for gateway {StoredGatewayIpAddress} ({StoredGatewayModel})", storedGateway.IpAddress, storedGateway.Model);

                var sensorsToRemove = storedGateway.Sensors.Where(s => updatedGateway.Sensors.All(gs => gs.Name != s.Name)).ToList();
                if (sensorsToRemove.Count > 0)
                {
                    emitDiscovery = true;
                    await EmitDiscoveryRemoval(storedGateway.Name, sensorsToRemove);
                }
                foreach (var sensor in sensorsToRemove)
                {
                    storedGateway.Sensors.Remove(sensor);
                }

                if (!_deviceStore.UpsertGateway(storedGateway))
                {
                    _logger.LogWarning("failed to update {StoredGatewayIpAddress} ({StoredGatewayModel}) in the store", storedGateway.IpAddress, storedGateway.Model);
                }
                else
                {
                    if(emitDiscovery) await EmitHomeAssistantDiscovery(storedGateway);
                    _logger.LogDebug("gateway updated: {Serialize})", JsonSerializer.Serialize(storedGateway));
                }
            }

            //LogStorageState();
        }

        public async Task OnHandle(SubdeviceApiAggregate message, CancellationToken cancellationToken)
        {
            var ips = message.Subdevices.DistinctBy(sd => sd.GwIp).Select(sd => sd.GwIp);
            foreach (var ip in ips)
            {
                var storedGateway = _deviceStore.GetGateway(ip);
                if (storedGateway == null)
                {
                    _logger.LogWarning("Gateway {Ip} not in store yet. Skipping subdevice update until gateway sends data", ip);
                    continue;
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
                        _logger.LogInformation("subdevice added: {DataId} ({DataModel})", data.Id, data.Model);

                        await EmitHomeAssistantDiscovery(storedGateway);
                        _deviceStore.UpsertGateway(storedGateway);
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
                        var flushData = false;
                        foreach (var sensor in updatedSubDevice.Sensors)
                        {
                            var storedSensor = storedSubDevice.Sensors.FirstOrDefault(s => s.Name == sensor.Name);
                            if (storedSensor == null)
                            {
                                sensor.DiscoveryUpdate = true;
                                storedSubDevice.Sensors.Add(sensor);
                                changedSensors.Add(sensor);
                                flushData = true;
                            }
                            else if (DeNoiserHelper.HasSignificantChange(storedSensor, sensor.Value))
                            {
                                storedSensor.Value = sensor.Value;
                                changedSensors.Add(storedSensor);
                            }
                        }

                        if (changedSensors.Count > 0) await EmitSubdeviceChanged(changedSensors, storedGateway.IpAddress, storedSubDevice.Id);
                        else _logger.LogInformation("no changes for subdevice {DataId} ({DataModel})", data.Id, data.Model);

                        var sensorsToRemove = storedSubDevice.Sensors.Where(s => updatedSubDevice.Sensors.All(us => us.Name != s.Name)).ToList();
                        if (sensorsToRemove.Count > 0)
                        {
                            flushData = true;
                            await EmitDiscoveryRemoval(storedSubDevice.Nickname, sensorsToRemove);
                        }
                        foreach (var sensor in sensorsToRemove)
                        {
                            storedSubDevice.Sensors.Remove(sensor);
                        }
                        
                        if (flushData)
                        {
                            _deviceStore.UpsertGateway(storedGateway);
                            await EmitHomeAssistantDiscovery(storedGateway);
                        }

                        _logger.LogInformation("subdevice updated: {DataId} ({DataModel})", data.Id, data.Model);
                    }
                }
            }

            //LogStorageState();
        }


        public Task OnHandle(HttpServiceEvent message, CancellationToken cancellationToken)
        {
            switch (message.EventType)
            {
                case HttpServiceEventType.Started:
                    _logger.LogInformation("HTTP Service started");
                    break;
                case HttpServiceEventType.Stopped:
                    _logger.LogWarning("HTTP Service stopped");
                    break;
                case HttpServiceEventType.Error:
                    _logger.LogError("HTTP Service error: {MessageMessage}", message.Message);
                    break;
                case HttpServiceEventType.Unknown:
                default:
                    _logger.LogWarning("Unknown HTTP Service event: {HttpServiceEventType}", message.EventType);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
