using Hairdresser.Data;
using Microsoft.AspNetCore.Mvc;

namespace Hairdresser.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context; // Veritabanı bağlamı
        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var employees = _context.Employees.ToList();
            if (employees == null)
            {
                // Debugging purposes
                Console.WriteLine("Employees is null");
            }
            ViewBag.Employees = employees;
            var appointments = _context.Appointments.ToList();
            if (appointments == null)
            {
                // Debugging purposes
                Console.WriteLine("Appointments is null");
            }
            ViewBag.Appointments = appointments;
            return View();
        }
        public IActionResult AddEmployee()
        {
            return View();
        }
        public IActionResult RemoveEmployee()
        {
            return View();
        }

    }
}
