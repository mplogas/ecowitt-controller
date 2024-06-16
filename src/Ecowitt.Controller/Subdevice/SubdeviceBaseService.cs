using Ecowitt.Controller.Configuration;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Ecowitt.Controller.Subdevice;

public abstract class SubdeviceBaseService  : IHostedService, IDisposable
{
    protected readonly ILogger _logger;
    protected readonly IMessageBus _messageBus;
    protected readonly IHttpClientFactory _httpClientFactory;
    protected readonly EcowittOptions _options;
    
    public SubdeviceBaseService(ILogger logger, IMessageBus messageBus, IHttpClientFactory httpClientFactory, IOptions<EcowittOptions> options)
    {
        _logger = logger;
        _messageBus = messageBus;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // TODO release managed resources here
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public abstract Task StartAsync(CancellationToken cancellationToken);
    public abstract Task StopAsync(CancellationToken cancellationToken);
}