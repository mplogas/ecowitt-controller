using Ecowitt.Controller.Message.Event;

namespace Ecowitt.Controller.Service.Orchestrator
{
    public partial class StateMachine
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
                    _logger.LogError($"MQTT Service error: {message.Message}");
                    _lastMqttServiceState = MqttServiceEventType.Error;
                    break;
                case MqttServiceEventType.Heartbeat:
                    _logger.LogDebug("MQTT Service heartbeat received");
                    _lastMqttServiceState = MqttServiceEventType.Heartbeat;
                    break;
                case MqttServiceEventType.Unknown:
                default:
                    _logger.LogWarning($"Unknown MQTT Service event: {message.EventType}");
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
                    _logger.LogInformation($"MQTT Client error {message.Message}");
                    break;
                case MqttConnectionEventType.MessageReceived:
                default:
                    _logger.LogInformation($"MQTT Connection event received: {message.EventType}");
                    break;
            }

            return Task.CompletedTask;
        }

        public Task OnHandle(HomeAssistantStatusEvent message)
        {
            switch (message.Status)
            {
                case HomeAssistantStatusType.Online:
                    _logger.LogInformation("HomeAssistant is online");
                    break;
                case HomeAssistantStatusType.Offline:
                    _logger.LogInformation("HomeAssistant is offline");
                    break;
                case HomeAssistantStatusType.Unknown:
                default:
                    _logger.LogInformation("Unknown HomeAssistant status received");
                    break;
            }

            return Task.CompletedTask;
        }

        

    }
}
