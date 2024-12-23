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
        private readonly AppDbContext _context; // Veritaban� ba�lam�
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
            // Veritaban�ndan hizmetleri �ek
            var services = _context.Services.ToList();

            // Se�ilen hizmet yoksa varsay�lan olarak ilk hizmeti se�
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
                        // Saatin mevcut randevularla �ak���p �ak��mad���n� kontrol et
                        var appointmentDate = selectedDate?.Date.Add(TimeSpan.Parse(hour));
                        return !_context.Appointments.Any(a =>
                            a.AppointmentEmployeeID == e.EmployeeID &&
                            a.AppointmentDate == appointmentDate);
                    }) // Saatleri liste olarak ay�r
                    .ToList()
                })
                .ToList();  // �ak��mayan saatleri listeye ekle

            // ViewBag kullanarak verileri View'e ta��
            ViewBag.Services = services; // T�m hizmetler
            ViewBag.SelectedService = selectedService; // Se�ilen hizmet
            ViewBag.SelectedDate = selectedDate; // Se�ilen tarih
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
                    // Kullan�c� ID'si kontrol et
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

                    // M�sait saat kontrol�
                    var availableHours = employee.AvailableHours.Split(',').Select(h => h.Trim()).ToList();
                    //if (!availableHours.Contains(selectedHour))
                    //{
                    //    TempData["ErrorMessage"] = "Selected time is not available!";
                    //    return RedirectToAction("Services");
                    //}

                    // Tarih ve saati birle�tirerek randevu tarihi olu�tur
                    DateTime appointmentDateTime = selectedDate.Date.Add(TimeSpan.Parse(selectedHour));

                    //// �ak��ma kontrol�
                    //var existingAppointment = _context.Appointments.FirstOrDefault(a =>
                    //        a.AppointmentEmployeeID == employee.EmployeeID &&
                    //        a.AppointmentDate.Date == appointmentDateTime.Date && // Tarih kar��la�t�rmas�
                    //        a.AppointmentDate.TimeOfDay == appointmentDateTime.TimeOfDay && // Saat kar��la�t�rmas�
                    //        a.AppointmentUserID == userId); // Ayn� kullan�c�

                    //if (existingAppointment != null)
                    //{
                    //    TempData["ErrorMessage"] = "The selected time is already booked!";
                    //    return RedirectToAction("Services");
                    //}

                    // Kullan�c� Randevusuyla �ak��ma Kontrol�
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

                    // Se�ilen saati �al��an�n saat listesinden ��kar
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

            // �al��an�n saatlerini b�l
            var availableHours = employee.AvailableHours.Split(',').ToList()
                .Where(hour =>
                {
                    // Se�ilen tarih ve saat i�in �ak��ma kontrol�
                    var appointmentDate = selectedDate.Date.Add(TimeSpan.Parse(hour));
                    return !_context.Appointments.Any(a =>
                        a.AppointmentEmployeeID == employee.EmployeeID &&
                        a.AppointmentDate == appointmentDate);
                })
                .ToList();

            return Json(new { success = true, availableHours });
        }

        // Foto�raf� y�klemek i�in GET metodu
        [HttpGet]
        public IActionResult UploadImage()
        {
            return View();
        }

        // Foto�raf� i�lemek ve RapidAPI'ye g�ndermek i�in POST metodu
        [HttpPost]
        public async Task<IActionResult> AnalyzeImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ViewData["APIResponse"] = "Please select a valid image file.";
                return View("Index"); // Ana sayfay� yeniden y�kler
            }
            if (!imageFile.ContentType.StartsWith("image/"))
            {
                return BadRequest("Invalid file format. Please upload an image.");
            }

            Console.WriteLine($"Received file: {imageFile.FileName}, Size: {imageFile.Length}");
            try
            {
                // Resim dosyas�n� ge�ici bir ak��a kopyala
                using var stream = new MemoryStream();
                await imageFile.CopyToAsync(stream);
                //byte[] imageData = stream.ToArray();
                stream.Position = 0;

                // Rapid API �zerinden �a�r� yapmak i�in servisinizi �a��r�n
                string apiResult = await _rapidAPIService.AnalyzeImageAsync(stream);
                if (string.IsNullOrEmpty(apiResult))
                {
                    Console.WriteLine("API returned an empty response.");
                    return BadRequest("API did not return any results.");
                }

                //// API sonucunu ViewData'ya ekle
                //ViewData["APIResponse"] = apiResult;
                return Json(apiResult); // JSON format�nda API sonucunu d�nd�r
            }
            catch (Exception ex)
            {
                //// Hata durumunda hata mesaj�n� ayarla
                //ViewData["APIResponse"] = $"Error: {ex.Message}";
                return Json($"Error: {ex.Message}");
            }

            //return View("Index"); // Ana sayfaya y�nlendir
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
