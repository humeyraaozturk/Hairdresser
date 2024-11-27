using Microsoft.AspNetCore.Mvc;

namespace Hairdresser.Controllers
{
    public class AppointmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
