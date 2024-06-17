using Ecowitt.Controller.Model;
using SlimMessageBus;

namespace Ecowitt.Controller.Consumer;

public class ApiDataConsumer : IConsumer<ApiData>
{
    public async Task OnHandle(ApiData message)
    {
        throw new NotImplementedException();
    }
}