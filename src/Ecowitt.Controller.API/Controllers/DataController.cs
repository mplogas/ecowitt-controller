using Microsoft.AspNetCore.Mvc;

namespace Ecowitt.Controller.Controllers
{
    public class DataController : Microsoft.AspNetCore.Mvc.Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
