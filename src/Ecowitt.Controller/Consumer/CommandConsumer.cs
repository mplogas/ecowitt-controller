using Ecowitt.Controller.Configuration;
using Ecowitt.Controller.Model;
using Ecowitt.Controller.Store;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SlimMessageBus;

namespace Ecowitt.Controller.Consumer;

public class CommandConsumer : IConsumer<SubdeviceApiCommand>
{
    private readonly ILogger<CommandConsumer> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDeviceStore _deviceStore;
    private readonly EcowittOptions _options;

    public CommandConsumer(ILogger<CommandConsumer> logger, IHttpClientFactory httpClientFactory, IDeviceStore store, IOptions<EcowittOptions> options)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _deviceStore = store;
        _options = options.Value;
    }

    public async Task OnHandle(SubdeviceApiCommand message)
    {
        _logger.LogInformation($"Received SubdeviceCommand: {message.Cmd} for device {message.Id}");
        
        var gw = _deviceStore.GetGatewayBySubdeviceId(message.Id);
        if(gw == null)
        {
            _logger.LogWarning($"Gateway not found for subdevice {message.Id}");
            return;
        }
        // hey compiler, this can't be null!
        var subdevice = gw.Subdevices.FirstOrDefault(sd => sd.Id == message.Id);

        if (message.Cmd == Command.Start)
        {
            var val = message.Duration ?? 0;
            var valType = message.Unit ?? 0;
            // the magic "maganiFator" :D
            if(valType == DurationUnit.Liters) val *= 10; 
            var alwaysOn = message.AlwaysOn.HasValue ? 1 : 0;
            
            await SendCommand(gw.IpAddress, "quick_run", message.Id, (int)subdevice!.Model, val: val, valType: (int)valType, alwaysOn: alwaysOn);
        } else if (message.Cmd == Command.Stop)
        {
            await SendCommand(gw.IpAddress, "quick_stop", message.Id, (int)subdevice!.Model);
        }
        else
        {
            _logger.LogWarning($"Ignoring unsupported command {message.Cmd} for subdevice {message.Id}");
            return;
        }
    }

    
    private async Task<bool> SendCommand(string ipAddress, string cmd, int id, int model, int val = 0, int valType = 0, int onType = 0, int offType = 0, int alwaysOn = 0, int onTime = 0, int offTime = 0)
    {
        var client = _httpClientFactory.CreateClient("ecowitt-client");
        client.BaseAddress = new Uri($"http://{ipAddress}");

        var username = _options.Gateways.FirstOrDefault(gw => gw.Ip == ipAddress)?.Username;
        var password = _options.Gateways.FirstOrDefault(gw => gw.Ip == ipAddress)?.Password;
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
            //TODO: authentication header, need to test
            //client.DefaultRequestHeaders.Add();
        }
        
        // [{"on_type":0,"off_type":0,"always_on":0,"on_time":0,"off_time":0,"val_type":1,"val":20,"cmd":"quick_run","id":13441,"model":1}]}    
        dynamic payload;
        switch (cmd)
        {
            case "quick_run":
                payload = new { command = new[] { new { cmd, id, model, val, val_type = valType, on_type = onType, off_type = offType, always_on = alwaysOn, on_time = onTime, off_time = offTime } } };
                break;
            case "quick_stop":
                payload = new { command = new[] { new { cmd, id, model } } };
                break;
            default:
                _logger.LogWarning($"Unsupported command type {cmd}. Not sending command to {ipAddress} for subdevice {id}");
                return false;
        }
        
        var sContent = new StringContent(JsonConvert.SerializeObject(payload));
        var response = await client.PostAsync("parse_quick_cmd_iot", sContent);

        if (response.IsSuccessStatusCode) return true;
        else {
            _logger.LogWarning($"Could not send command to {ipAddress} for subdevice {id}");
            return false;
        }
    }
}