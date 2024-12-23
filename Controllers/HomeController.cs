using Hairdresser.Data;
using Hairdresser.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Hairdresser.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context; // Veritabaný baðlamý
        private readonly RapidAPIService _rapidAPIService;
        public HomeController(AppDbContext context, RapidAPIService rapidAPIService)
        {
            _context = context;
            _rapidAPIService = rapidAPIService;
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

        public IActionResult Services(int? selectedServiceId, DateTime? selectedDate)
        {
            // Veritabanýndan hizmetleri çek
            var services = _context.Services.ToList();

            // Seçilen hizmet yoksa varsayýlan olarak ilk hizmeti seç
            var selectedService = services.FirstOrDefault(s => s.ServiceID == selectedServiceId) ?? services.First();

            var employees = _context.Employees
                .Where(e => e.EmployeeServiceID == selectedService.ServiceID)
                .ToList()
                .Select(e => new
                {
                    e.EmployeeID,
                    e.FullName,
                    AvailableHours = e.AvailableHours.Split(',').ToList()
                    .Where(hour =>
                    {
                        // Saatin mevcut randevularla çakýþýp çakýþmadýðýný kontrol et
                        var appointmentDate = selectedDate?.Date.Add(TimeSpan.Parse(hour));
                        return !_context.Appointments.Any(a =>
                            a.AppointmentEmployeeID == e.EmployeeID &&
                            a.AppointmentDate == appointmentDate);
                    }) // Saatleri liste olarak ayýr
                    .ToList()
                })
                .ToList();  // Çakýþmayan saatleri listeye ekle

            // ViewBag kullanarak verileri View'e taþý
            ViewBag.Services = services; // Tüm hizmetler
            ViewBag.SelectedService = selectedService; // Seçilen hizmet
            ViewBag.SelectedDate = selectedDate; // Seçilen tarih
            ViewBag.Employees = employees;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveAppointment(string ServiceName, string EmployeeId, DateTime selectedDate, string selectedHour)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kullanýcý ID'si kontrol et
                    var userId = HttpContext.Session.GetString("UserID");
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["ErrorMessage"] = "User not logged in!";
                        return RedirectToAction("Login");
                    }

                    var service = _context.Services.FirstOrDefault(s => s.Name == ServiceName);
                    if (service == null)
                    {
                        TempData["ErrorMessage"] = "Service not found!";
                        return RedirectToAction("Services");
                    }

                    var employee = _context.Employees.FirstOrDefault(e => e.EmployeeID == EmployeeId);
                    if (employee == null)
                    {
                        TempData["ErrorMessage"] = "Employee not found!";
                        return RedirectToAction("Services");
                    }

                    // Müsait saat kontrolü
                    var availableHours = employee.AvailableHours.Split(',').Select(h => h.Trim()).ToList();
                    //if (!availableHours.Contains(selectedHour))
                    //{
                    //    TempData["ErrorMessage"] = "Selected time is not available!";
                    //    return RedirectToAction("Services");
                    //}

                    // Tarih ve saati birleþtirerek randevu tarihi oluþtur
                    DateTime appointmentDateTime = selectedDate.Date.Add(TimeSpan.Parse(selectedHour));

                    //// Çakýþma kontrolü
                    //var existingAppointment = _context.Appointments.FirstOrDefault(a =>
                    //        a.AppointmentEmployeeID == employee.EmployeeID &&
                    //        a.AppointmentDate.Date == appointmentDateTime.Date && // Tarih karþýlaþtýrmasý
                    //        a.AppointmentDate.TimeOfDay == appointmentDateTime.TimeOfDay && // Saat karþýlaþtýrmasý
                    //        a.AppointmentUserID == userId); // Ayný kullanýcý

                    //if (existingAppointment != null)
                    //{
                    //    TempData["ErrorMessage"] = "The selected time is already booked!";
                    //    return RedirectToAction("Services");
                    //}

                    // Kullanýcý Randevusuyla Çakýþma Kontrolü
                    var existingAppointmentForUser = _context.Appointments.FirstOrDefault(a =>             
                        a.AppointmentDate == appointmentDateTime && 
                        a.AppointmentUserID == userId);

                    if (existingAppointmentForUser != null)
                    {
                        TempData["ErrorMessage"] = "You already have an appointment on the selected day and time";
                        return RedirectToAction("Appointments", "User");
                    }

                    var appointment = new Appointment
                    {
                        AppointmentServiceID = service.ServiceID,
                        AppointmentEmployeeID = EmployeeId,
                        AppointmentUserID = userId,
                        AppointmentDate = appointmentDateTime,
                        Status = "Pending",
                        TotalPrice = service.Price
                    };

                    _context.Appointments.Add(appointment);

                    // Seçilen saati çalýþanýn saat listesinden çýkar
                    availableHours.Remove(selectedHour);
                    employee.AvailableHours = string.Join(",", availableHours);
                    _context.Employees.Update(employee);

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Appointment successfully created!";
                    return RedirectToAction("Appointments","User");
                }
                catch (Exception ex)
                {
                    // Hata loglama ve mesaj
                    TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                    return RedirectToAction("Appointments", "User");
                }
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ErrorMessage"] = "Invalid input data: " + string.Join("; ", errors);
                return RedirectToAction("Appointments", "User");
            }


            TempData["ErrorMessage"] = "Invalid input data!";
            return RedirectToAction("Appointments", "User");
        }

        [HttpGet]
        public JsonResult GetEmployeeAvailableHours(string employeeId, DateTime selectedDate)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeID == employeeId);

            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found." });
            }

            // Çalýþanýn saatlerini böl
            var availableHours = employee.AvailableHours.Split(',').ToList()
                .Where(hour =>
                {
                    // Seçilen tarih ve saat için çakýþma kontrolü
                    var appointmentDate = selectedDate.Date.Add(TimeSpan.Parse(hour));
                    return !_context.Appointments.Any(a =>
                        a.AppointmentEmployeeID == employee.EmployeeID &&
                        a.AppointmentDate == appointmentDate);
                })
                .ToList();

            return Json(new { success = true, availableHours });
        }

        // Fotoðrafý yüklemek için GET metodu
        [HttpGet]
        public IActionResult UploadImage()
        {
            return View();
        }

        // Fotoðrafý iþlemek ve RapidAPI'ye göndermek için POST metodu
        [HttpPost]
        public async Task<IActionResult> AnalyzeImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ViewData["APIResponse"] = "Please select a valid image file.";
                return View("Index"); // Ana sayfayý yeniden yükler
            }
            if (!imageFile.ContentType.StartsWith("image/"))
            {
                return BadRequest("Invalid file format. Please upload an image.");
            }

            Console.WriteLine($"Received file: {imageFile.FileName}, Size: {imageFile.Length}");
            try
            {
                // Resim dosyasýný geçici bir akýþa kopyala
                using var stream = new MemoryStream();
                await imageFile.CopyToAsync(stream);
                //byte[] imageData = stream.ToArray();
                stream.Position = 0;

                // Rapid API üzerinden çaðrý yapmak için servisinizi çaðýrýn
                string apiResult = await _rapidAPIService.AnalyzeImageAsync(stream);
                if (string.IsNullOrEmpty(apiResult))
                {
                    Console.WriteLine("API returned an empty response.");
                    return BadRequest("API did not return any results.");
                }

                //// API sonucunu ViewData'ya ekle
                //ViewData["APIResponse"] = apiResult;
                return Json(apiResult); // JSON formatýnda API sonucunu döndür
            }
            catch (Exception ex)
            {
                //// Hata durumunda hata mesajýný ayarla
                //ViewData["APIResponse"] = $"Error: {ex.Message}";
                return Json($"Error: {ex.Message}");
            }

            //return View("Index"); // Ana sayfaya yönlendir
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
