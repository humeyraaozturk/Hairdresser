using Microsoft.AspNetCore.Mvc;

namespace Hairdresser.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }
        public IActionResult Details()
        {
            return View();
        }
        public IActionResult Edit()
        {
            return View();
        }
    }
}
