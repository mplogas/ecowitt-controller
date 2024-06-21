using System.Net;
using System.Reflection;
using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Consumer;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Mqtt;
using Ecowitt.Controller.Store;
using Ecowitt.Controller.Subdevice;
using MQTTnet;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Serilog;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Memory;
using SlimMessageBus.Host.Serialization.Json;

namespace Ecowitt.Controller;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(File.Exists("/config/appsettings.json") ? "/config" : builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();

        builder.Configuration.Sources.Clear();
        builder.Configuration.AddConfiguration(configuration);
        builder.Services.Configure<EcowittOptions>(configuration.GetSection("ecowitt"));
        builder.Services.Configure<MqttOptions>(configuration.GetSection("mqtt"));
        builder.Services.Configure<ControllerOptions>(configuration.GetSection("controller"));

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        builder.Services.AddLogging(c => c.AddSerilog().AddConsole().AddDebug());

        builder.Services.AddSingleton<IDeviceStore, DeviceStore>();

        builder.Services.AddSlimMessageBus(smb =>
        {
            smb.WithProviderMemory(cfg => { cfg.EnableMessageSerialization = true; });
            smb.AddJsonSerializer();
            smb.Produce<ApiData>(x => x.DefaultTopic("api-data"));
            smb.Produce<SubdeviceData>(x => x.DefaultTopic("subdevice-data"));
            smb.Produce<SubdeviceCommand>(x => x.DefaultTopic("subdevice-command"));
            smb.Consume<ApiData>(x => x
                .Topic("api-data")
                .WithConsumer<DataConsumer>()
            );
            smb.Consume<SubdeviceData>(x => x
                .Topic("subdevice-data")
                .WithConsumer<DataConsumer>()
            );
            smb.Consume<SubdeviceCommand>(x => x
                .Topic("subdevice-command")
                .WithConsumer<CommandConsumer>()
            );
            smb.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        builder.Services.AddTransient<MqttFactory>();
        builder.Services.AddSingleton<IMqttClient, MqttClient>();
        builder.Services.AddHostedService<MqttService>();

        var gateways = configuration.GetSection("ecowitt:gateways").Get<List<GatewayOptions>>();

        if (gateways == null) builder.Services.AddHttpClient();
        else
            foreach (var gw in gateways)
                builder.Services.AddHttpClient($"ecowitt-client-{gw.Name}", client =>
                {
                    var uriBuilder = new UriBuilder
                    {
                        Scheme = "http",
                        Host = gw.Ip,
                        Port = gw.Port
                    };
                    client.BaseAddress = uriBuilder.Uri;
                }).AddPolicyHandler(GetRetryPolicy(gw.Retries));

        builder.Services.AddHostedService<SubdeviceService>();

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        // // Configure the HTTP request pipeline.
        // if (app.Environment.IsDevelopment())
        // {
        //     app.UseSwagger();
        //     app.UseSwaggerUI();
        // }

        app.MapControllers();
        await app.RunAsync();
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retries)
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(2), retries, fastFirst: true);

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
            //.WaitAndRetryAsync(retries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            .WaitAndRetryAsync(delay);
    }
}