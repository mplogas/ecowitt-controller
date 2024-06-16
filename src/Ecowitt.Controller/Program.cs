using System.Net.Mime;
using System.Reflection;
using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Mqtt;
using Ecowitt.Controller.Subdevice;
using Ecowitt.Controller.Validator;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using MQTTnet;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Serilog;
using SlimMessageBus;
using SlimMessageBus.Host;
using SlimMessageBus.Host.FluentValidation;
using SlimMessageBus.Host.Memory;
using SlimMessageBus.Host.Serialization.Json;

namespace Ecowitt.Controller
{
    public class Program
    {
        public static async Task Main(string[] args)
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
                smb.AddAspNet();
                smb.AddFluentValidation(cfg =>
                {
                    cfg.AddProducerValidatorsFromAssemblyContaining<ApiDataValidator>();
                    //if i ever want to map the validation errors into a custom exception
                    //cfg.AddValidationErrorsHandler(errors => new ApplicationException("Custom Validation Exception"));
                });
                smb.Produce<ApiData>(x => x.DefaultTopic("api-data"));
                smb.Produce<SubdeviceData>(x => x.DefaultTopic("subdevice-data"));
                smb.Produce<SubdeviceCommand>(x => x.DefaultTopic("subdevice-command"));
                smb.Consume<ApiData>(x => x
                    .Topic("api-data")
                    .WithConsumer<MqttService>()
                );
                smb.Consume<SubdeviceData>(x => x
                    .Topic("subdevice-data")
                    .WithConsumer<MqttService>()
                );
                smb.Consume<SubdeviceCommand>(x => x
                    .Topic("subdevice-command")
                    .WithConsumer<SubdeviceService>()
                );
            });
            //builder.Services.AddValidatorsFromAssemblyContaining<ApiDataValidator>();
            builder.Services.AddScoped<IValidator<ApiData>, ApiDataValidator>();
            
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

            // Required for SlimMessageBus.Host.AspNetCore package
            builder.Services.AddHttpContextAccessor();
            
            //builder.Services.AddControllers();
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Translates the ValidationException into a 400 bad request
            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    if (exceptionHandlerPathFeature?.Error is ValidationException e)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        context.Response.ContentType = MediaTypeNames.Application.Json;
                        await context.Response.WriteAsJsonAsync(new { e.Errors });
                    }
                });
            });
            
            
            
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.MapPost("report/data", (ApiData data, IMessageBus bus) => bus.Publish(data));

            await app.RunAsync();
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
