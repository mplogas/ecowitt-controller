using Ecowitt.Controller.Message.Config;
using Ecowitt.Controller.Message.Data;
using Ecowitt.Controller.Message.Event;
using MQTTnet.Client;

namespace Ecowitt.Controller.Service.Mqtt
{
    public partial class MqttService
    {
        public async Task OnHandle(MqttConfig message)
        {
            _logger.LogInformation($"{_serviceId}: handle mqttConfig");
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
            EmitHomeAssistantDiscovery();
        }

        public Task OnHandle(DeviceData message)
        {
            if (_client == null || !_client.IsConnected)
            {
                _logger.LogWarning("MQTT client is not connected. Cannot publish device data.");
            }

            return Task.CompletedTask;
        }
    }
}
