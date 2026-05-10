using Ecowitt.Controller.Model.Message.Config;
using Ecowitt.Controller.Model.Message.Data;

namespace Ecowitt.Controller.Service.Http
{
    public partial class HttpPublishingService
    {
        public Task OnHandle(HttpConfig message, CancellationToken cancellationToken)
        {
            _logger.LogDebug("HttpConfig received");
            _config = message ?? throw new ArgumentNullException(nameof(message), "HttpConfig cannot be null");

            return Task.CompletedTask;
        }

        public Task OnHandle(SubdeviceCommandDispatch message, CancellationToken cancellationToken)
            => SendSubdeviceCommand(message.GatewayIp, message.Command, message.Model, cancellationToken);
    }
}
