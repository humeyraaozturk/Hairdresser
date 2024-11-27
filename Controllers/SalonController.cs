using Microsoft.AspNetCore.Mvc;

namespace Hairdresser.Controllers
{
    public class SalonController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
