using Hairdresser.Data;
using Hairdresser.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Hairdresser.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context; // Veritabaný baðlamý

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }

        //public IActionResult Services()
        //{
        //    return View();
        //}

        public IActionResult Services(int? selectedServiceId)
        {
            // Veritabanýndan hizmetleri çek
            var services = _context.Services.ToList();

            // Seçilen hizmet yoksa varsayýlan olarak ilk hizmeti seç
            var selectedService = services.FirstOrDefault(s => s.ServiceID == selectedServiceId) ?? services.First();

            var employees = _context.Employees.Where(e => e.EmployeeServiceID == selectedService.ServiceID).ToList();

            // ViewBag kullanarak verileri View'e taþý
            ViewBag.Services = services; // Tüm hizmetler
            ViewBag.SelectedService = selectedService; // Seçilen hizmet

            ViewBag.Employees = employees;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveAppointment(string ServiceName, string EmployeeId, DateTime AppointmentDate)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var service = _context.Services.FirstOrDefault(s => s.Name == ServiceName);
                    if (service == null)
                    {
                        TempData["ErrorMessage"] = "Service not found!";
                        return RedirectToAction("Services");
                    }

                    // Kullanýcý ID'si kontrol et
                    var userId = HttpContext.Session.GetString("UserID");
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["ErrorMessage"] = "User not logged in!";
                        return RedirectToAction("Login");
                    }

                    var appointment = new Appointment
                    {
                        AppointmentServiceID = service.ServiceID,
                        AppointmentEmployeeID = EmployeeId,
                        AppointmentUserID = userId,
                        AppointmentDate = AppointmentDate,
                        Status = "Pending",
                        TotalPrice = service.Price
                    };

                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Appointment successfully created!";
                    return RedirectToAction("Services");
                }
                catch (Exception ex)
                {
                    // Hata loglama ve mesaj
                    TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                    return RedirectToAction("Services");
                }
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ErrorMessage"] = "Invalid input data: " + string.Join("; ", errors);
                return RedirectToAction("Services");
            }


            TempData["ErrorMessage"] = "Invalid input data!";
            return RedirectToAction("Services");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
