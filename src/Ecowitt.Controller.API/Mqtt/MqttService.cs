namespace Ecowitt.Controller.Mqtt
{
    public class MqttService : IHostedService
    {
        private readonly ILogger<MqttService> logger;
        private readonly MqttClient mqttClient;
        private readonly IConfigurationSection mqttConfig;
        private readonly IServiceProvider services;
        private readonly string statusTopic;
        private readonly string heartbeatTopic;
        private readonly string responseTopic;

        public MqttService(IConfiguration config, ILogger<MqttService> logger, IServiceProvider services, IMqttClient mqttClient)
        {
            this.logger = logger;
            this.services = services;
            this.mqttConfig = config.GetSection("mqtt");
            if (!this.mqttConfig.Exists()) throw new Exception("missing mqtt configuration");

            this.statusTopic = $"{mqttConfig.GetValue<string>("topic")}/status/";
            this.heartbeatTopic = $"{mqttConfig.GetValue<string>("topic")}/heartbeat/";
            this.responseTopic = $"{mqttConfig.GetValue<string>("topic")}/response/";

            this.mqttClient = (MqttClient)mqttClient; //TODO: i feel bad for the hard cast
            this.mqttClient.OnMessageReceived += OnMessageReceived;
            this.mqttClient.OnClientDisconnected += OnClientDisconnected;
            this.mqttClient.OnClientConnected += OnClientConnected;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.mqttClient.Connect(this.mqttConfig.GetValue<string>("host"), this.mqttConfig.GetValue<string>("clientid"),
                this.mqttConfig.GetValue<string>("user"), this.mqttConfig.GetValue<string>("password"));
            this.logger.LogInformation("Connected");

            await mqttClient.Subscribe($"{this.statusTopic}#");
            await mqttClient.Subscribe($"{this.heartbeatTopic}#");
            await mqttClient.Subscribe($"{this.responseTopic}#");
            this.logger.LogInformation("subscribed to all topics");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this.mqttClient.Disconnect();
            this.logger.LogInformation("Disconnected");
        }

        private void OnClientConnected(object sender, EventArgs e)
        {
            this.logger.LogInformation("onconnect");
        }

        private void OnClientDisconnected(object sender, EventArgs e)
        {
            this.logger.LogInformation("ondisconnect");
        }

        private async void OnMessageReceived(object sender, MqttMessageReceivedEventArgs e)
        {
            //await using var scope = this.services.CreateAsyncScope();
            //var scopedHub = scope.ServiceProvider.GetRequiredService<IHubContext<MessageHub>>();

            //if (e.Topic.StartsWith(this.statusTopic))
            //{
            //    var definition = new { device_id = string.Empty, value = 0, spool_id = string.Empty };
            //    var payload = JsonConvert.DeserializeAnonymousType(e.Payload, definition);
            //    if (!string.IsNullOrWhiteSpace(payload.device_id))
            //    {
            //        if (Guid.TryParse(payload.spool_id, out var spoolId))
            //        {
            //            if (payload.value > 0)
            //            {
            //                var scopedDbContext = scope.ServiceProvider.GetRequiredService<SmartMassDbContext>();
            //                var entry = new HistoryEventDto(spoolId, payload.value, DateTime.UtcNow);
            //                scopedDbContext.MqttValues.Add(entry);
            //                await scopedDbContext.SaveChangesAsync();
            //            }
            //            await scopedHub.Clients.All.SendAsync("KnownStatus", payload.device_id, payload.value, spoolId);
            //        }
            //        else
            //        {
            //            await scopedHub.Clients.All.SendAsync("Status", payload.device_id, payload.value);
            //        }
            //    }
            //}
            //else if (e.Topic.StartsWith(this.heartbeatTopic))
            //{
            //    var definition = new { device_id = string.Empty, status = "N/A" };
            //    var payload = JsonConvert.DeserializeAnonymousType(e.Payload, definition);
            //    if (!string.IsNullOrWhiteSpace(payload.device_id))
            //    {
            //        var scopedDbContext = scope.ServiceProvider.GetRequiredService<SmartMassDbContext>();
            //        var discoveredDevices = scope.ServiceProvider.GetRequiredService<IDiscoveredDevices>();

            //        if (!discoveredDevices.Contains(payload.device_id) &&
            //            !scopedDbContext.Devices.Any(d => d.ClientId == payload.device_id))
            //        {
            //            discoveredDevices.Add(payload.device_id);
            //        }
            //        else if (scopedDbContext.Devices.Any(d => d.ClientId == payload.device_id))
            //        {
            //            // cleanup 
            //            if (discoveredDevices.Contains(payload.device_id)) discoveredDevices.Remove(payload.device_id);

            //            //only notify the client for registered devices
            //            await scopedHub.Clients.All.SendAsync("Heartbeat", payload.device_id, payload.status);
            //        }
            //    }
            //}
            //else if (e.Topic.StartsWith(this.responseTopic))
            //{
            //    var definition = new { device_id = string.Empty, result = 0 };
            //    var payload = JsonConvert.DeserializeAnonymousType(e.Payload, definition);
            //    if (!string.IsNullOrWhiteSpace(payload.device_id))
            //    {
            //        await scopedHub.Clients.All.SendAsync("Response", payload.device_id, payload.result);
            //    }
            //}
            //else
            //{
            //    this.logger.LogWarning($"message received from {e.ClientId} on topic {e.Topic}. Payload: {e.Payload}");
            //}

        }
    }
}
