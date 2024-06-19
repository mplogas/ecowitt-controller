using Ecowitt.Controller.Model;
using SlimMessageBus;

namespace Ecowitt.Controller.Consumer;

public class DataConsumer : IConsumer<ApiData>, IConsumer<SubdeviceData>
{
    private readonly ILogger<DataConsumer> _logger;

    public DataConsumer(ILogger<DataConsumer> logger)
    {
        this._logger = logger;
    }
    public async Task OnHandle(ApiData message)
    {
        _logger.LogInformation($"Received ApiData: {message.PASSKEY}");
    }
    
    public async Task OnHandle(SubdeviceData message)
    {
        _logger.LogInformation($"Received data for {message.Subdevices.Count} subdevices");
    }
}