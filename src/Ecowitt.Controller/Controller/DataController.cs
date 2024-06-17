using Ecowitt.Controller.Model;
using Microsoft.AspNetCore.Mvc;
using SlimMessageBus;

namespace Ecowitt.Controller.Controller;

[ApiController]
[Route("[controller]")]
public class DataController : ControllerBase
{
    private readonly ILogger<DataController> _logger;
    private readonly IMessageBus _messageBus;

    public DataController(ILogger<DataController> logger, IMessageBus messageBus)
    {
        this._logger = logger;
        this._messageBus = messageBus;
    }
    
    [HttpPost]
    [ActionName("report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostData([FromQuery] ApiData data)
    {
        //TODO: add plausibility checks here
        
        this._logger.LogInformation("Received data from {StationType} station.", data.StationType);
        await this._messageBus.Publish(data);

        return Ok();
    }
}