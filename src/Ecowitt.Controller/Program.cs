using System.Net;
using System.Reflection;
using Ecowitt.Controller.Model.Api;
using Ecowitt.Controller.Model.Configuration;
using Ecowitt.Controller.Model.Message.Config;
using Ecowitt.Controller.Model.Message.Data;
using Ecowitt.Controller.Model.Message.Event;
using Ecowitt.Controller.Service.Http;
using Ecowitt.Controller.Service.Mqtt;
using Ecowitt.Controller.Service.Orchestrator;
using MQTTnet;
using Newtonsoft.Json;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Core.Enrichers;
using Serilog.Events;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Memory;
using SlimMessageBus.Host.Serialization.Json;

namespace Ecowitt.Controller;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();
        
        var myEnv = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(File.Exists("/config/appsettings.json") ? "/config" : builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{myEnv}.json", true, true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        builder.Configuration.Sources.Clear();
        builder.Configuration.AddConfiguration(configuration);
        builder.Services.Configure<EcowittOptions>(configuration.GetSection("ecowitt"));
        builder.Services.Configure<MqttOptions>(configuration.GetSection("mqtt"));
        builder.Services.Configure<ControllerOptions>(configuration.GetSection("controller"));

        var ecowittRetries = configuration.GetSection("ecowitt").GetValue<int>("retries");
        builder.Services.AddHttpClient("ecowitt-client").AddPolicyHandler(GetRetryPolicy(ecowittRetries > 0 ? ecowittRetries : 2));

        builder.Services.AddSerilog((services, lc) => lc
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(Path.Exists("/config/logs") ? "/config/logs/ecowitt-controller.log" : "logs/ecowitt-controller.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5)
            .MinimumLevel.Warning()
            .ReadFrom.Configuration(builder.Configuration));


        builder.Services.AddSingleton<IDeviceStore, DeviceStore>();
        builder.Services.AddSingleton<MqttService>();
        builder.Services.AddSingleton<HttpPublishingService>();
        builder.Services.AddSingleton<Dispatcher>();

        builder.Services.AddSlimMessageBus(smb =>
        {
            smb.WithProviderMemory(cfg => { cfg.EnableMessageSerialization = true; });
            smb.AddJsonSerializer(jsonSerializerSettings: JsonSettings);

            // statemachine -> mqttservice
            smb.Produce<MqttConfig>(x => x.DefaultTopic("config-mqtt"));
            smb.Produce<HomeAssistantDiscoveryEvent>(x => x.DefaultTopic("home-assistant-discovery"));
            smb.Produce<DiscoveryRemovalEvent>(x => x.DefaultTopic("discovery-removal"));
            smb.Produce<DeviceData>(x => x.DefaultTopic("device-data"));
            smb.Produce<DeviceDataFull>(x => x.DefaultTopic("device-data-full"));
            smb.Produce<SubdeviceData>(x => x.DefaultTopic("subdevice-data"));
            smb.Produce<SubdeviceDataFull>(x => x.DefaultTopic("subdevice-data-full"));
            smb.Consume<MqttConfig>(x => x.Topic("config-mqtt").WithConsumer<MqttService>());
            smb.Consume<HomeAssistantDiscoveryEvent>(x => x.Topic("home-assistant-discovery").WithConsumer<MqttService>());
            smb.Consume<DiscoveryRemovalEvent>(x => x.Topic("discovery-removal").WithConsumer<MqttService>());
            smb.Consume<DeviceData>(x => x.Topic("device-data").WithConsumer<MqttService>());
            smb.Consume<DeviceDataFull>(x => x.Topic("device-data-full").WithConsumer<MqttService>());
            smb.Consume<SubdeviceData>(x => x.Topic("subdevice-data").WithConsumer<MqttService>());
            smb.Consume<SubdeviceDataFull>(x => x.Topic("subdevice-data-full").WithConsumer<MqttService>());

            // mqttservice -> statemachine
            smb.Produce<MqttServiceEvent>(x => x.DefaultTopic("mqtt-service-event"));
            smb.Produce<MqttConnectionEvent>(x => x.DefaultTopic("mqtt-connection-event"));
            smb.Produce<HomeAssistantStatusEvent>(x => x.DefaultTopic("home-assistant-status"));
            smb.Consume<MqttServiceEvent>(x => x.Topic("mqtt-service-event").WithConsumer<Dispatcher>());
            smb.Consume<MqttConnectionEvent>(x => x.Topic("mqtt-connection-event").WithConsumer<Dispatcher>());
            smb.Consume<HomeAssistantStatusEvent>(x => x.Topic("home-assistant-status").WithConsumer<Dispatcher>());

            // controller -> statemachine
            smb.Produce<GatewayApiData>(x => x.DefaultTopic("gw-api-data"));
            smb.Consume<GatewayApiData>(x => x.Topic("gw-api-data").WithConsumer<Dispatcher>());

            // statemachine -> HttpPublishingService
            smb.Produce<HttpConfig>(x => x.DefaultTopic("config-http"));
            smb.Consume<HttpConfig>(x => x.Topic("config-http").WithConsumer<HttpPublishingService>());

            // HttpPublishingService -> statemachine
            smb.Produce<SubdeviceApiAggregate>(x => x.DefaultTopic("subdevice-api-data"));
            smb.Produce<HttpServiceEvent>(x => x.DefaultTopic("http-service-event"));
            smb.Consume<SubdeviceApiAggregate>(x => x.Topic("subdevice-api-data").WithConsumer<Dispatcher>());
            smb.Consume<HttpServiceEvent>(x => x.Topic("http-service-event").WithConsumer<Dispatcher>());

            // mqttservice -> statemachine (subdevice commands)
            smb.Produce<SubdeviceApiCommand>(x => x.DefaultTopic("subdevice-api-command"));
            smb.Consume<SubdeviceApiCommand>(x => x.Topic("subdevice-api-command").WithConsumer<Dispatcher>());

            smb.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        builder.Services.AddHostedService(s => s.GetRequiredService<Dispatcher>());
        
        builder.Services.AddTransient<MqttClientFactory>();
        builder.Services.AddHostedService(s => s.GetRequiredService<MqttService>());
        builder.Services.AddHostedService(s => s.GetRequiredService<HttpPublishingService>());

        builder.Services.AddControllers();

        var app = builder.Build();
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "Handled {RequestPath}";
            options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            };
        });
        
        app.MapControllers();
        
        await app.RunAsync();
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retries)
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(2), retries, fastFirst: true);

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryAsync(delay);
    }

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        NullValueHandling = NullValueHandling.Ignore
    };
}