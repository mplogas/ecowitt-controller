using Ecowitt.Controller.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SlimMessageBus;

namespace Ecowitt.Controller.Controller;

[ApiController]
//[Route("[controller]")]
public class DataController : ControllerBase
{
    private readonly ILogger<DataController> _logger;
    private readonly IMessageBus _messageBus;

    public DataController(ILogger<DataController> logger, IMessageBus messageBus)
    {
        _logger = logger;
        _messageBus = messageBus;
    }

    // [Route("**")]
    // public IActionResult CatchAll()
    // {
    //     var qryParams = Request.Query.Count();
    //     var formParams = Request.Form.Count();
    //     var bodyParams = Request.Body.Length;
    //     
    //     _logger.LogInformation($"Received data from {Request.Path} with {qryParams} query parameters, {formParams} form parameters and {bodyParams} bytes in body.");
    //     
    //     return Ok();
    // }

    [HttpGet("/data/report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return new JsonResult(new { status = "ok" });
    }

    [HttpPost("/data/report")]
    [Consumes("application/x-www-form-urlencoded")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostData([FromForm] GatewayApiData data)
    {
        var ip = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        if (string.IsNullOrWhiteSpace(ip))
        {
            _logger.LogWarning("Could not determine IP address of request.");
        }
        else

        {
            _logger.LogInformation($"Received data from IP {ip} ({data.StationType}).");
            data.IpAddress = ip;
        }

        //write forms key/values into Payload property as json
        data.Payload =
            JsonConvert.SerializeObject(Request.Form.Select(kvp => new { name = kvp.Key, value = kvp.Value[0] }));
        _logger.LogDebug($"Request form keys: {string.Join(", ", Request.Form.Keys)}");

        await _messageBus.Publish(data);

        return Ok();
    }
}