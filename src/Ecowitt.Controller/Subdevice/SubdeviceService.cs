using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Model;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Ecowitt.Controller.Subdevice;

internal class SubdeviceService : SubdeviceBaseService, IConsumer<SubdeviceCommand>
{
    public SubdeviceService(ILogger<SubdeviceDiscoveryService> logger, IMessageBus messageBus,
        IHttpClientFactory httpClientFactory, IOptions<EcowittOptions> options)
        : base(logger, messageBus, httpClientFactory, options)
    {
    }

    public async Task OnHandle(SubdeviceCommand message)
    {
        //throw new NotImplementedException();
        _logger.LogInformation($"Received SubdeviceCommand: {message.Cmd} for device {message.Id}");
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        //throw new NotImplementedException();
        _logger.LogInformation("Starting SubdeviceService");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        //throw new NotImplementedException();
        _logger.LogInformation("Stopping SubdeviceService");
    }
}