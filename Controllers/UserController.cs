using Microsoft.AspNetCore.Mvc;

namespace Hairdresser.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
