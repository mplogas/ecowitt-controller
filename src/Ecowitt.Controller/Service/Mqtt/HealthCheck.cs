using Ecowitt.Controller.Service.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ecowitt.Controller.Service.Mqtt;

public class HealthCheck : IHealthCheck
{
    private readonly HttpPublishingService _service;

    public HealthCheck(HttpPublishingService service)
    {
        _service = service;
    }
    
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}