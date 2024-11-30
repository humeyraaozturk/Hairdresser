using Microsoft.AspNetCore.Mvc;

namespace Hairdresser.Controllers
{
    public class SalonController : Controller
    {
        public IActionResult Details()
        {
            return View();
        }
        public IActionResult Edit()
        {
            return View();
        }
        public IActionResult EditServices()
        {
            return View();
        }
        public IActionResult AddServices()
        {
            return View();
        }
        public IActionResult ManageServices()
        {
            return View();
        }
    }
}
