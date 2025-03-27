using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Store;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Ecowitt.Controller;

public class StateMachine : IConsumer<SubdeviceApiCommand>, IConsumer<GatewayApiData>, IConsumer<SubdeviceApiAggregate>
{
    public StateMachine(ILogger<StateMachine> logger, IDeviceStore deviceStore, IHttpClientFactory httpClientFactory, IOptions<EcowittOptions> ecowittOptions, IOptions<ControllerOptions> controllerOptions) 
    {
        
    }

    public async Task OnHandle(SubdeviceApiCommand message)
    {
        throw new NotImplementedException();
    }

    public async Task OnHandle(GatewayApiData message)
    {
        throw new NotImplementedException();
    }

    public async Task OnHandle(SubdeviceApiAggregate message)
    {
        throw new NotImplementedException();
    }
}