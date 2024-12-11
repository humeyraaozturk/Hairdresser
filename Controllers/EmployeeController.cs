using Hairdresser.Data;
using Hairdresser.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hairdresser.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}    

        [HttpPost]
        [ValidateAntiForgeryToken]
     
        public IActionResult Details()
        {
            return View();
        }
        
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.Service) // Service ile ilişki varsa
                .ToListAsync();

            return View(employees);
        }
    }
}
