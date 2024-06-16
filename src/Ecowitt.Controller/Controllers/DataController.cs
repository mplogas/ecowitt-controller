using Ecowitt.Controller.Model;
using Microsoft.AspNetCore.Mvc;

namespace Ecowitt.Controller.Controllers
{
    [ApiController]
    public class DataController : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostData([FromQuery] ApiData data)
        {
            return Ok();
        }
    }
}
