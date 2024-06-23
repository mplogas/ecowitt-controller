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
            
        builder.Services.AddSerilog((services, lc) => lc
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                //.WriteTo.Console()
                .WriteTo.Debug()
                .WriteTo.File("logs/ecowitt-controller.log", rollingInterval: RollingInterval.Day)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Debug)
                .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Debug));

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
        builder.Services.AddHostedService<SubdeviceService>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        app.UseSerilogRequestLogging(options =>
        {
            // Customize the message template
            options.MessageTemplate = "Handled {RequestPath}";
    
            // Emit debug-level events instead of the defaults
            options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;
    
            // Attach additional properties to the request completion event
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            };
        });
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
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