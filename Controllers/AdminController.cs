using Microsoft.AspNetCore.Mvc;

namespace Hairdresser.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult ManageAppointments()
        {
            return View();
        }
        public IActionResult ManageEmployees()
        {
            return View();
        }
        public IActionResult ManageUsers()
        {
            return View();
        }
    }
}
