using Microsoft.AspNetCore.Mvc;

namespace Hairdresser.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Appointments()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Profile()
        {
            return View();
        }
    }
}
