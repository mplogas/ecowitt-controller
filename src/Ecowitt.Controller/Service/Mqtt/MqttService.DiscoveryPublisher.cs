
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Model.Api;
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

        private async Task PublishSubdeviceRunModeDiscovery(Device gw, Ecowitt.Controller.Model.Subdevice subdevice)
        {
            var modes = RunModeRegistry.ApplicableModes(subdevice.Model, subdevice.HasFlowMeter);
            if (modes.Count == 0) return;

            var device = DiscoveryBuilder.BuildDevice(subdevice.Nickname, subdevice.Model.ToString(), "Ecowitt",
                subdevice.Model.ToString(), subdevice.Version.ToString(), DiscoveryBuilder.BuildIdentifier(gw.Name));
            var baseTopic = $"{_mqttConfig?.BaseTopic}/{MqttPathBuilder.BuildMqttSubdeviceTopic(gw.Name, subdevice.Id.ToString())}";
            var nick = MqttPathBuilder.SanitizeSegment(subdevice.Nickname);

            // Mode select — only when 2+ modes apply.
            if (modes.Count > 1)
            {
                var selId = DiscoveryBuilder.BuildIdentifier(subdevice.Nickname, "runmode");
                var selCfg = DiscoveryBuilder.BuildSelectConfig(device, _origin, "Run Mode", selId,
                    commandTopic: $"{baseTopic}/cmd/mode",
                    options: modes.Select(m => m.HaLabel).ToList());
                await PublishDiscoveryMessage("select", $"select/{nick}", selCfg);
            }

            // One number per applicable mode (config category).
            foreach (var m in modes)
            {
                var param = m.Key == RunModeKey.Volume ? "volume" : "duration";
                var unit = m.Key == RunModeKey.Volume ? "L" : "min";
                var numId = DiscoveryBuilder.BuildIdentifier(subdevice.Nickname, $"run{param}");
                var numCfg = DiscoveryBuilder.BuildNumberConfig(device, _origin, $"Run {m.HaLabel}", numId,
                    commandTopic: $"{baseTopic}/cmd/set/{param}",
                    min: m.Min, max: m.Max, step: 1, unitOfMeasurement: unit, entityCategory: "config");
                await PublishDiscoveryMessage("number", $"number/{nick}_{param}", numCfg);
            }

            // Start button.
            var btnId = DiscoveryBuilder.BuildIdentifier(subdevice.Nickname, "runstart");
            var btnCfg = DiscoveryBuilder.BuildButtonConfig(device, _origin, "Start Run", btnId,
                commandTopic: $"{baseTopic}/cmd/start");
            await PublishDiscoveryMessage("button", $"button/{nick}", btnCfg);

            // Publish the CURRENT staged values (retained) so HA reflects them and they survive a
            // controller restart. Must use the staged config, NOT m.Default — otherwise a discovery
            // re-emission (e.g. on HA restart) would clobber a user's staged value back to default.
            var staged = subdevice.StagedRunConfig;
            if (modes.Count > 1)
                await Publish($"{baseTopic}/cmd/mode", RunModeRegistry.ByKey(staged.Mode).HaLabel, true);
            foreach (var m in modes)
            {
                var param = m.Key == RunModeKey.Volume ? "volume" : "duration";
                var current = m.Key == RunModeKey.Volume ? staged.VolumeLiters : staged.DurationMinutes;
                await Publish($"{baseTopic}/cmd/set/{param}", current.ToString(), true);
            }
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

            // HA voltage/temperature/etc default to integer display unless told otherwise; mirror the
            // publish-side rounding precision so e.g. soilbatt 1.4V isn't rendered as "1 V".
            int? displayPrecision = sensor.DataType == SensorDataType.Double ? (_mqttConfig?.Precision ?? 2) : null;

            var config = sensor.SensorCategory == SensorCategory.Diagnostic
                ? DiscoveryBuilder.BuildSensorConfig(device, _origin, sensor.Alias, id, category, statetopic, valueTemplate: valueTemplate, unitOfMeasurement: sensor.UnitOfMeasurement, sensorCategory: sensor.SensorCategory.ToString().ToLower(), isBinarySensor: sensor.SensorClass == SensorClass.BinarySensor, displayPrecision: displayPrecision)
                : DiscoveryBuilder.BuildSensorConfig(device, _origin, sensor.Alias, id, category, statetopic, valueTemplate: valueTemplate, unitOfMeasurement: sensor.UnitOfMeasurement, isBinarySensor: sensor.SensorClass == SensorClass.BinarySensor, displayPrecision: displayPrecision);

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
