using System.Reflection;
using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Mqtt;
using Ecowitt.Controller.Subdevice;
using MQTTnet;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Serilog;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Memory;
using SlimMessageBus.Host.Serialization.Json;

namespace Ecowitt.Controller
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(File.Exists("/config/appsettings.json") ? "/config" : builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
                .Build();

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddConfiguration(configuration);
            builder.Services.Configure<EcowittOptions>(configuration.GetSection("ecowitt"));
            builder.Services.Configure<MqttOptions>(configuration.GetSection("mqtt"));

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            builder.Services.AddLogging(c => c.AddSerilog().AddConsole());

            builder.Services.AddSlimMessageBus(smb =>
            {
                smb.WithProviderMemory(cfg =>
                {
                    cfg.EnableMessageSerialization = true;
                });
                smb.AddJsonSerializer();
                smb.Produce<ApiData>(x => x.DefaultTopic("api-data"));
                smb.Produce<SubdeviceData>(x => x.DefaultTopic("subdevice-data"));
                smb.Produce<SubdeviceCommand>(x => x.DefaultTopic("subdevice-command"));
                smb.Consume<ApiData>(x => x
                    .Topic("api-data")
                    .WithConsumer<ApiDataConsumer>()
                );
                smb.Consume<SubdeviceData>(x => x
                    .Topic("subdevice-data")
                    .WithConsumer<SubdeviceDataConsumer>()
                );
                smb.Consume<SubdeviceCommand>(x => x
                    .Topic("subdevice-command")
                    .WithConsumer<SubdeviceCommandConsumer>()
                );
            });

            builder.Services.AddTransient<MqttFactory>();
            builder.Services.AddSingleton<IMqttClient, MqttClient>();
            builder.Services.AddHostedService<MqttService>();

            builder.Services.AddHttpClient("ecowitt-client", client =>
            {
                client.BaseAddress =
                    new Uri(configuration.GetValue<string>("ecowitt:endpoint") ?? "http://localhost:5000");
                var apiKey = configuration.GetValue<string>("endpoint:apikey") ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    client.DefaultRequestHeaders.Add("Authorization", apiKey);
                }
            }).AddPolicyHandler(GetRetryPolicy(configuration.GetValue<int>("ecowitt:retries")));
            builder.Services.AddHostedService<SubdeviceDiscoveryService>();
            builder.Services.AddHostedService<SubdeviceService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retries)
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: retries, fastFirst: true);

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                //.WaitAndRetryAsync(retries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
                .WaitAndRetryAsync(delay);
        }

    }
}
