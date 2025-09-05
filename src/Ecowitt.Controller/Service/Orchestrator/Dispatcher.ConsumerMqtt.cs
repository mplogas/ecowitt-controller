using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Message.Event;

namespace Ecowitt.Controller.Service.Orchestrator
{
    public partial class Dispatcher
    {
        public Task OnHandle(MqttServiceEvent message)
        {
            switch (message.EventType)
            {
                case MqttServiceEventType.Started:
                    _logger.LogInformation("MQTT Service started");
                    _lastMqttServiceState = MqttServiceEventType.Started;
                    //_mqttServiceStarted.TrySetResult(true);
                    break;
                case MqttServiceEventType.Stopped:
                    _logger.LogWarning("MQTT Service stopped");
                    _lastMqttServiceState = MqttServiceEventType.Stopped;
                    break;
                case MqttServiceEventType.Error:
                    _logger.LogError("MQTT Service error: {MessageMessage}", message.Message);
                    _lastMqttServiceState = MqttServiceEventType.Error;
                    break;
                case MqttServiceEventType.Heartbeat:
                    _logger.LogDebug("MQTT Service heartbeat received");
                    _lastMqttServiceState = MqttServiceEventType.Heartbeat;
                    break;
                case MqttServiceEventType.Unknown:
                default:
                    _logger.LogWarning("Unknown MQTT Service event: {MqttServiceEventType}", message.EventType);
                    break;
            }

            return Task.CompletedTask;
        }

        public Task OnHandle(MqttConnectionEvent message)
        {
            switch (message.EventType)
            {
                case MqttConnectionEventType.Connected:
                    _logger.LogInformation("MQTT Client connected");
                    break;
                case MqttConnectionEventType.Disconnected:
                    _logger.LogWarning("MQTT Client disconnected");
                    break;
                case MqttConnectionEventType.Error:
                    _logger.LogInformation("MQTT Client error {MessageMessage}", message.Message);
                    break;
                case MqttConnectionEventType.MessageReceived:
                default:
                    _logger.LogInformation("MQTT Connection event received: {MqttConnectionEventType}", message.EventType);
                    break;
            }

            return Task.CompletedTask;
        }

        public async Task OnHandle(HomeAssistantStatusEvent message)
        {
            switch (message.Status)
            {
                case HomeAssistantStatusType.Online:
                    _logger.LogWarning("HomeAssistant is online");
                    foreach (var gateway in _deviceStore.GetGatewaysShort().Select(kvp => _deviceStore.GetGateway(kvp.Key)).OfType<Device>())
                    {
                        //await EmitHomeAssistantDiscovery(gateway);
                        await EmitGatewayFull(gateway);
                        foreach (var subdevice in gateway.Subdevices)
                        {
                            await EmitSubdeviceFull(subdevice);
                        }

                        _logger.LogInformation("Re-emitted device {deviceIp}", gateway.IpAddress);
                    }
                    break;
                case HomeAssistantStatusType.Offline:
                    _logger.LogWarning("HomeAssistant is offline");
                    break;
                case HomeAssistantStatusType.Unknown:
                default:
                    _logger.LogInformation("Unknown HomeAssistant status received");
                    break;
            }
        }

        

    }
}
