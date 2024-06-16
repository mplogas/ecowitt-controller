using Ecowitt.Controller.Configuration;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Ecowitt.Controller.Subdevice
{
    public class SubdeviceDiscoveryService : SubdeviceBaseService
    {
        public SubdeviceDiscoveryService(ILogger<SubdeviceDiscoveryService> logger, IMessageBus messageBus, IHttpClientFactory httpClientFactory, IOptions<EcowittOptions> options) : base(logger, messageBus, httpClientFactory, options)
        {
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
