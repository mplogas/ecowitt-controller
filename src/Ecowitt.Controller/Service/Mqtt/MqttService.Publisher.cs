using Ecowitt.Controller.Model;
using MQTTnet;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using MQTTnet.Protocol;

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
                    JsonSerializer.Serialize(payload, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }), retain: true))
                _logger.LogWarning("Failed to publish message to topic {MqttConfigBaseTopic}/{Topic}. Is the client connected?", _mqttConfig?.BaseTopic, topic);
        }

        private async Task PublishAvailabilityMessage(string topic, DateTime timestamp)
        {
            var available = DateTime.UtcNow.Subtract(timestamp) < TimeSpan.FromSeconds(300) ? "online" : "offline";

            if (!await Publish($"{_mqttConfig?.BaseTopic}/{topic}/availability", available, retain: true))
                _logger.LogWarning("Failed to publish message to topic {MqttConfigBaseTopic}/{Topic}. Is the client connected?", _mqttConfig?.BaseTopic, topic);
        }

        private async Task<bool> Publish(string topic, string message, bool retain = false)
        {
            var mqttPayload = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithRetainFlag(retain)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            if (_client is { IsConnected: true })
            {
                var result = await _client.PublishAsync(mqttPayload);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Published message to topic {topic}", topic);
                    _logger.LogDebug("Message {message} ", message);
                    return true;
                }
                else _logger.LogWarning("Failed to publish message to topic {Topic}. Reason: {MqttClientPublishReasonCode}", topic, result.ReasonCode);
            }
            else _logger.LogWarning("Can't publish message to topic {Topic}. Client not connected.", topic);
            return false;
        }
    }
}
