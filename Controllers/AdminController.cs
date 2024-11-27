using Microsoft.AspNetCore.Mvc;

namespace Hairdresser.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
