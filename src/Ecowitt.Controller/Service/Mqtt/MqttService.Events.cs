using Ecowitt.Controller.Model.Api;
using Ecowitt.Controller.Model.Message.Event;
using MQTTnet;
using MQTTnet.Client;
using System.Text.Json;

namespace Ecowitt.Controller.Service.Mqtt
{
    public partial class MqttService
    {
        public async Task StartedAsync(CancellationToken cancellationToken)
        {
            await _messageBus.Publish(new MqttServiceEvent { EventType = MqttServiceEventType.Started }, cancellationToken: cancellationToken);
        }

        public Task StartingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task StoppedAsync(CancellationToken cancellationToken)
        {
            await _messageBus.Publish(new MqttServiceEvent { EventType = MqttServiceEventType.Stopped }, cancellationToken: cancellationToken);
        }

        public Task StoppingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task ClientOnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            var payload = arg.ApplicationMessage.ConvertPayloadToString();
            var topic = arg.ApplicationMessage.Topic;
            _logger.LogDebug("Message received for topic {Topic}: {Payload}", topic, payload);

            if (topic.Equals(HaStatusTopic, StringComparison.OrdinalIgnoreCase))
            {
                // home assistant status topic
                if (payload.Equals("online", StringComparison.OrdinalIgnoreCase))
                    await _messageBus.Publish(new HomeAssistantStatusEvent() { Status = HomeAssistantStatusType.Online });
                else if (payload.Equals("offline", StringComparison.OrdinalIgnoreCase)) await _messageBus.Publish(new HomeAssistantStatusEvent { Status = HomeAssistantStatusType.Offline });
                else await _messageBus.Publish(new HomeAssistantStatusEvent { Status = HomeAssistantStatusType.Unknown });
            }
            else if (topic.EndsWith("cmd/homeassistant"))
            {
                // commands coming from home assistant
                if (int.TryParse(topic.Split('/')[3], out var result))
                {
                    var cmd = payload.Equals("ON", StringComparison.InvariantCultureIgnoreCase) ? Command.Start : Command.Stop; //I know, everything that's not "ON" is "OFF"
                    await _messageBus.Publish(new SubdeviceApiCommand() { Cmd = cmd, Id = result });
                }
                else
                {
                    _logger.LogWarning("Invalid subdevice id in topic {Topic}", topic);
                }
            }
            else
            {
                // direct commands via mqtt
                try
                {
                    var cmd = JsonSerializer.Deserialize<SubdeviceApiCommand>(payload);
                    if (cmd == null)
                    {
                        _logger.LogWarning("Failed to deserialize command from topic {Topic}", topic);
                        return;
                    }
                    await _messageBus.Publish(cmd);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to deserialize command from topic {Topic}: {Payload}", topic, payload);
                }
            }
        }

        private async Task ClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            _logger.LogInformation("MQTT client disconnected.");
            await _messageBus.Publish<MqttConnectionEvent>(new MqttConnectionEvent { EventType = MqttConnectionEventType.Disconnected });
            if (_mqttConfig is { Reconnect: true } && !_isConnecting)
            {
                _logger.LogInformation("Attempting to reconnect to MQTT broker...");
                _isConnecting = true;
                try
                {
                    for (var i = 0; i < _mqttConfig.ReconnectAttempts; i++)
                    {
                        try
                        {
                            var result = await _client?.ConnectAsync(_client.Options)!;
                            if (result.ResultCode == MqttClientConnectResultCode.Success) break;
                            else _logger.LogWarning("Failed to reconnect to MQTT broker. Attempt {I} of {MqttConfigReconnectAttempts}. Reason: {MqttClientConnectResultCode}", i + 1, _mqttConfig.ReconnectAttempts, result.ResultCode);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Exception during MQTT reconnect attempt {I} of {MqttConfigReconnectAttempts}: {ExMessage}", i + 1, _mqttConfig.ReconnectAttempts, ex.Message);
                        }
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }
                finally
                {
                    _isConnecting = false;
                }
            }
        }

        private async Task ClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            _logger.LogInformation("MQTT client connected.");
            await _messageBus.Publish<MqttConnectionEvent>(new MqttConnectionEvent
            {
                EventType = MqttConnectionEventType.Connected
            });
        }
    }
}
