using Ecowitt.Controller.Model;
using SlimMessageBus;

namespace Ecowitt.Controller.Consumer;

public class CommandConsumer : IConsumer<SubdeviceCommand>
{
    private readonly ILogger<CommandConsumer> _logger;

    public CommandConsumer(ILogger<CommandConsumer> logger)
    {
        _logger = logger;
    }

    public async Task OnHandle(SubdeviceCommand message)
    {
        _logger.LogInformation($"Received SubdeviceCommand: {message.Cmd} for device {message.Id}");
    }
}