using Ecowitt.Controller.Model;
using MQTTnet;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ecowitt.Controller.Service.Mqtt
{
    public partial class MqttService
    {
        private async Task PublishSubdevice(Subdevice subdevice, string gatewayName)
        {
            var payload = MqttPayloadBuilder.BuildSubdevicePayload(subdevice);
            await PublishMessage(MqttPathBuilder.BuildMqttSubdeviceTopic(gatewayName, subdevice.Id.ToString()), payload);
        }

        private async Task PublishSubdeviceSensors(List<ISensor> sensors, string gatewayName, int subdeviceId)
        {
            var precision = _mqttConfig?.Precision ?? 2;
            foreach (var sensor in sensors)
            {
                var payload = MqttPayloadBuilder.BuildSensorPayload(sensor, precision);
                var topic = sensor.SensorCategory == SensorCategory.Diagnostic ? MqttPathBuilder.BuildMqttSubdeviceDiagnosticTopic(gatewayName, subdeviceId.ToString(), sensor.Alias) : MqttPathBuilder.BuildMqttSubdeviceSensorTopic(gatewayName, subdeviceId.ToString(), sensor.Alias);
                await PublishMessage(topic, payload);
            }
        }

        private async Task PublishGateway(Device gateway)
        {
            var payload = MqttPayloadBuilder.BuildGatewayPayload(gateway);
            await PublishMessage(MqttPathBuilder.BuildMqttGatewayTopic(gateway.Name), payload);
        }

        private async Task PublishSensors(List<ISensor> sensors, string gatewayName )
        {
            var precision = _mqttConfig?.Precision ?? 2;
            foreach (var sensor in sensors)
            {
                var payload = MqttPayloadBuilder.BuildSensorPayload(sensor, precision);
                var topic = sensor.SensorCategory == SensorCategory.Diagnostic ? MqttPathBuilder.BuildMqttGatewayDiagnosticTopic(gatewayName, sensor.Alias) : MqttPathBuilder.BuildMqttGatewaySensorTopic(gatewayName, sensor.Alias);
                await PublishMessage(topic, payload);
            }
        }

        private async Task PublishMessage(string topic, dynamic payload)
        {
            if (!await Publish($"{_mqttConfig?.BaseTopic}/{topic}",
                    JsonSerializer.Serialize(payload, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping })))
                _logger.LogWarning($"Failed to publish message to topic {_mqttConfig?.BaseTopic}/{topic}. Is the client connected?");
        }

        private async Task PublishAvailabilityMessage(string topic, DateTime timestamp)
        {
            var available = DateTime.UtcNow.Subtract(timestamp) < TimeSpan.FromSeconds(300) ? "online" : "offline";

            if (!await Publish($"{_mqttConfig?.BaseTopic}/{topic}/availability", available))
                _logger.LogWarning($"Failed to publish message to topic {_mqttConfig?.BaseTopic}/{topic}. Is the client connected?");
        }

        private async Task<bool> Publish(string topic, string message)
        {
            var mqttPayload = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .Build();

            if (_client is { IsConnected: true })
            {
                var result = await _client.PublishAsync(mqttPayload);
                if (result.IsSuccess)
                {
                    _logger.LogDebug($"Message {message} published to topic {topic}");
                    return true;
                }
                else _logger.LogWarning($"Failed to publish message {message} to topic {topic}. Reason: {result.ReasonCode}");
            }
            else _logger.LogWarning($"Can't publish message {message} to topic {topic}. Client not connected.");
            return false;
        }
    }
}
