using Ecowitt.Controller.Model;
using SlimMessageBus;

namespace Ecowitt.Controller.Consumer;

public class SubdeviceDataConsumer : IConsumer<SubdeviceData>
{
    public async Task OnHandle(SubdeviceData message)
    {
        throw new NotImplementedException();
    }
}