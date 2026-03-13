
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Discovery;
using MQTTnet;
using MQTTnet.Protocol;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Device = Ecowitt.Controller.Model.Device;

namespace Ecowitt.Controller.Service.Mqtt
{
    public partial class MqttService
    {
        private async Task PublishGatewayDiscovery(Device gw)
        {
            var device = gw.Model == null ? DiscoveryBuilder.BuildDevice(gw.Name) : DiscoveryBuilder.BuildDevice(gw.Name, gw.Model, "Ecowitt", gw.Model, gw.StationType ?? "unknown");
            var id = DiscoveryBuilder.BuildIdentifier(gw.Name, "availability");
            //var statetopic = $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttGatewayTopic(gw.Name)}";
            var availabilityTopic = $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttGatewayTopic(gw.Name)}/availability";

            var config = DiscoveryBuilder.BuildGatewayConfig(device, _origin, "Availability", id, availabilityTopic, availabilityTopic);

            await PublishDiscoveryMessage("sensor", $"sensor/{MqttPathBuilder.SanitizeSegment(gw.Name)}", config);
        }

        private async Task PublishSubdeviceDiscovery(Device gw, Ecowitt.Controller.Model.Subdevice subdevice)
        {
            var device = DiscoveryBuilder.BuildDevice(subdevice.Nickname, subdevice.Model.ToString(), "Ecowitt", subdevice.Model.ToString(), subdevice.Version.ToString(), DiscoveryBuilder.BuildIdentifier(gw.Name));
            var id = DiscoveryBuilder.BuildIdentifier(subdevice.Nickname, "availability");
            //var statetopic = $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceTopic(gw.Name, subdevice.Id.ToString())}";
            var availabilityTopic = $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceTopic(gw.Name, subdevice.Id.ToString())}/availability";

            var config = DiscoveryBuilder.BuildGatewayConfig(device, _origin, "Availability", id, availabilityTopic, availabilityTopic);

            await PublishDiscoveryMessage("sensor", $"sensor/{MqttPathBuilder.SanitizeSegment(subdevice.Nickname)}", config);
        }

        private async Task PublishSubdeviceSwitchDiscovery(Device gw, Ecowitt.Controller.Model.Subdevice subdevice)
        {
            var device = DiscoveryBuilder.BuildDevice(subdevice.Nickname, subdevice.Model.ToString(), "Ecowitt", subdevice.Model.ToString(), subdevice.Version.ToString(), DiscoveryBuilder.BuildIdentifier(gw.Name));
            var id = DiscoveryBuilder.BuildIdentifier(subdevice.Nickname, "switch");
            var statetopic = $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceTopic(gw.Name, subdevice.Id.ToString())}/diag/running";
            var valueTemplate = "{% if (value_json.value == true) -%} ON {%- else -%} OFF {%- endif %}";
            var cmdTopic = $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceHACommandTopic(gw.Name, subdevice.Id.ToString())}";

            var config =
                DiscoveryBuilder.BuildSwitchConfig(device, _origin, "switch", id, statetopic, cmdTopic, valueTemplate: valueTemplate);

            await PublishDiscoveryMessage("switch", $"switch/{MqttPathBuilder.SanitizeSegment(subdevice.Nickname)}", config);
        }

        private async Task PublishSensorDiscovery(Device gw, ISensor sensor)
        {
            var device = gw.Model == null ? DiscoveryBuilder.BuildDevice(gw.Name) : DiscoveryBuilder.BuildDevice(gw.Name, gw.Model, "Ecowitt", gw.Model, gw.StationType ?? "unknown");

            var statetopic = sensor.SensorCategory == SensorCategory.Diagnostic ? $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttGatewayDiagnosticTopic(gw.Name, sensor.Alias)}" : $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttGatewaySensorTopic(gw.Name, sensor.Alias)}";
            var availabilityTopic = $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttGatewayTopic(gw.Name)}/availability";
            await PublishSensorDiscovery(device, sensor, statetopic, availabilityTopic);
        }

        private async Task PublishSensorDiscovery(Device gw, Ecowitt.Controller.Model.Subdevice subdevice, ISensor sensor)
        {
            var device = DiscoveryBuilder.BuildDevice(subdevice.Nickname, subdevice.Model.ToString(), "Ecowitt", subdevice.Model.ToString(), subdevice.Version.ToString(), DiscoveryBuilder.BuildIdentifier(gw.Name));
            var statetopic = sensor.SensorCategory == SensorCategory.Diagnostic ? $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceDiagnosticTopic(gw.Name, subdevice.Id.ToString(), sensor.Alias)}" : $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceSensorTopic(gw.Name, subdevice.Id.ToString(), sensor.Alias)}";
            var availabilityTopic = $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceTopic(gw.Name, subdevice.Id.ToString())}/availability";
            await PublishSensorDiscovery(device, sensor, statetopic, availabilityTopic);
        }

        private async Task PublishSensorDiscovery(Model.Discovery.Device device, ISensor sensor, string statetopic, string availabilityTopic)
        {
            var id = DiscoveryBuilder.BuildIdentifier($"{device.Name}_{sensor.Name}", sensor.SensorType.ToString());
            var category = DiscoveryBuilder.BuildDeviceCategory(sensor.SensorType);

            var valueTemplate = sensor.SensorClass == SensorClass.BinarySensor
                ? "{% if (value_json.value == true) -%} ON {%- else -%} OFF {%- endif %}"
                : "{{ value_json.value }}";
            //var valueTemplate = "{{ value_json.value }}";

            var config = sensor.SensorCategory == SensorCategory.Diagnostic
                ? DiscoveryBuilder.BuildSensorConfig(device, _origin, sensor.Alias, id, category, statetopic, valueTemplate: valueTemplate, unitOfMeasurement: sensor.UnitOfMeasurement, sensorCategory: sensor.SensorCategory.ToString().ToLower(), isBinarySensor: sensor.SensorClass == SensorClass.BinarySensor)
                : DiscoveryBuilder.BuildSensorConfig(device, _origin, sensor.Alias, id, category, statetopic, valueTemplate: valueTemplate, unitOfMeasurement: sensor.UnitOfMeasurement, isBinarySensor: sensor.SensorClass == SensorClass.BinarySensor);

            var sensorClassTopic = BuildSensorClassTopic(sensor.SensorClass);

            await PublishDiscoveryMessage(sensorClassTopic, $"{sensorClassTopic}/{MqttPathBuilder.SanitizeSegment($"{device.Name}_{sensor.Name}")}", config);
        }

        private async Task PublishDiscoveryMessage(string domain, string topic, Config config)
        {
            if (_client is { IsConnected: true })
            {
                config.DefaultEntityId = $"{domain}.{config.DefaultEntityId}";
                topic = $"homeassistant/{topic}/config";

                if (config.DeviceClass != null &&
                    config.DeviceClass.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                    config.DeviceClass = null;

                var payload = JsonSerializer.Serialize(config,
                    new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });

                await Publish(topic, payload, true);
            }
        }

        public async Task RemoveDiscoveryMessage(string topic, bool retain = false)
        {
            await Publish(topic, string.Empty, true);
        }

        private string BuildSensorClassTopic(SensorClass sc)
        {
            return sc switch
            {
                SensorClass.BinarySensor => "binary_sensor",
                _ => "sensor"
            };
        }
    }
}
