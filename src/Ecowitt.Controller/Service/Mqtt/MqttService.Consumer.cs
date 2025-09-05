using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Message.Config;
using Ecowitt.Controller.Model.Message.Data;
using Ecowitt.Controller.Model.Message.Event;
using MQTTnet.Client;

namespace Ecowitt.Controller.Service.Mqtt
{
    public partial class MqttService
    {
        public async Task OnHandle(MqttConfig message)
        {
            if (_client != null)
            {
                try
                {
                    if (_mqttConfig is { HomeAssistantDiscovery: true }) await UnsubscribeHomeAssistant();

                    if (_client.IsConnected) await _client.DisconnectAsync();
                    _client.ApplicationMessageReceivedAsync -= ClientOnApplicationMessageReceivedAsync;
                    _client.DisconnectedAsync -= ClientOnDisconnectedAsync;
                    _client.ConnectedAsync -= ClientOnConnectedAsync;
                    _client.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while disconnecting, unregistering or disposing MQTT client.");
                    return;
                }
            }

            _mqttConfig = message;

            if (!message.Enabled)
            {
                _logger.LogInformation("MQTT is disabled");
                return;
            }

            _client = _factory.CreateMqttClient();
            _client.ConnectedAsync += ClientOnConnectedAsync;
            _client.DisconnectedAsync += ClientOnDisconnectedAsync;
            _client.ApplicationMessageReceivedAsync += ClientOnApplicationMessageReceivedAsync;
            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttConfig.Host, _mqttConfig.Port)
                .WithClientId(_mqttConfig.ClientId)
                .WithCleanSession();
            if (!string.IsNullOrWhiteSpace(_mqttConfig.User))
                optionsBuilder.WithCredentials(_mqttConfig.User, _mqttConfig.Password);

            await _client.ConnectAsync(optionsBuilder.Build());
            await _client.SubscribeAsync($"{_mqttConfig.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceCommandTopic()}");

            if (_mqttConfig.HomeAssistantDiscovery) await SubscribeHomeAssistant();
        }

        public async Task OnHandle(HomeAssistantDiscoveryEvent message)
        {
            await EmitHomeAssistantDiscovery(message.Device);
        }

        public async Task OnHandle(DeviceData message)
        {
            if (_client == null || !_client.IsConnected)
            {
                _logger.LogWarning("MQTT client is not connected. Cannot publish device data.");
                return;
            }

            if (string.IsNullOrWhiteSpace(message.GatewayName))
            {
                _logger.LogWarning("Gateway name is empty. Cannot publish device data.");
                return;
            }

            await PublishSensors(message.ChangedSensors, message.GatewayName);
            await PublishAvailabilityMessage(MqttPathBuilder.BuildMqttGatewayTopic(message.GatewayName), message.Timestamp);
        }

        public async Task OnHandle(DeviceDataFull message)
        {
            if (_client == null || !_client.IsConnected)
            {
                _logger.LogWarning("MQTT client is not connected. Cannot publish device data.");
                return;
            }
            var gateway = message.Device;
            if (string.IsNullOrWhiteSpace(gateway.Name))
            {
                _logger.LogWarning("Gateway name is empty. Cannot publish device data.");
                return;
            }

            await PublishGateway(gateway);
            await PublishSensors(gateway.Sensors, gateway.Name);
            await PublishAvailabilityMessage(MqttPathBuilder.BuildMqttGatewayTopic(gateway.Name), message.Timestamp);
        }

        public async Task OnHandle(SubdeviceData message)
        {
            if (_client == null || !_client.IsConnected)
            {
                _logger.LogWarning("MQTT client is not connected. Cannot publish device data.");
                return;
            }

            if (string.IsNullOrWhiteSpace(message.GatewayName))
            {
                _logger.LogWarning("Gateway name is empty. Cannot publish device data.");
                return;
            }

            await PublishSubdeviceSensors(message.ChangedSensors, message.GatewayName, message.SubdeviceId);
            await PublishAvailabilityMessage(MqttPathBuilder.BuildMqttSubdeviceTopic(message.GatewayName, message.SubdeviceId.ToString()), DateTime.UtcNow);
        }

        public async Task OnHandle(SubdeviceDataFull message)
        {
            if (_client == null || !_client.IsConnected)
            {
                _logger.LogWarning("MQTT client is not connected. Cannot publish device data.");
                return;
            }

            if (string.IsNullOrWhiteSpace(message.GatewayName))
            {
                _logger.LogWarning("Gateway name is empty. Cannot publish device data.");
                return;
            }

            var subdevice = message.Subdevice;
            await PublishSubdevice(subdevice, message.GatewayName);
            await PublishSubdeviceSensors(message.Subdevice.Sensors, message.GatewayName, message.SubdeviceId);
            await PublishAvailabilityMessage(MqttPathBuilder.BuildMqttSubdeviceTopic(message.GatewayName, message.SubdeviceId.ToString()), DateTime.UtcNow);
        }
    }
}
