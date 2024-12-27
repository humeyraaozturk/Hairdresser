using Hairdresser.Data;
using Hairdresser.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Text.Json.Nodes;

namespace Hairdresser.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context; // Veritabaný baðlamý
        private readonly HttpClient _httpClient;
        private const string ApiKey = "cm52n6jhm0001ks03dmpl1vmv";
        public HomeController(AppDbContext context, HttpClient HttpClient)
        {
            _context = context;
            _httpClient = HttpClient;
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
                    
                    // Tarih ve saati birleþtirerek randevu tarihi oluþtur
                    DateTime appointmentDateTime = selectedDate.Date.Add(TimeSpan.Parse(selectedHour));

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
                    return RedirectToAction("Appointments", "User");
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

        // POST: Kullanýcýdan gelen veriyi API'ye gönder
        [HttpPost]
        public async Task<IActionResult> AnalyzeImage(string imageUrl, string editingType, string colorDescription, string hairstyleDescription)
        {
            // API URL
            string ApiUrl = "https://api.magicapi.dev/api/v1/magicapi/hair/hair";
            // API'ye gönderilecek veri
            var requestData = new
            {
                image = imageUrl,
                editing_type = editingType,
                color_description = colorDescription,
                hairstyle_description = hairstyleDescription
            };
            try
            {
                // API'ye POST isteði gönder
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json")
                };

                // x-magicapi-key ve Content-Type baþlýklarýný ekle
                requestMessage.Headers.Add("x-magicapi-key", ApiKey);
                //requestMessage.Content.Headers.Add("Content-Type", "application/json");

                // API'ye istek gönder
                var response = await _httpClient.SendAsync(requestMessage);

                // Dönüþ verisini JSON olarak al
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Baþarýlý dönüþ
                if (response.IsSuccessStatusCode)
                {
                    // API'den gelen yanýtý iþleme
                    dynamic json = JsonConvert.DeserializeObject(jsonResponse);
                    string requestId = json?.request_id;

                    // request_id deðerini konsola yazdýr
                    Console.WriteLine($"Request ID: {requestId}");

                    // request_id'yi TempData'ya ekleyelim
                    TempData["RequestId"] = requestId;

                    // Baþarýyla alýnan request_id deðerini GET metoduna gönder
                    return RedirectToAction("GetResult");
                }
                else
                {
                    // Hata durumu: API'den dönen hata mesajýný konsola yazdýr
                    Console.WriteLine($"API Error: {jsonResponse}");
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda hata mesajýný konsola yazdýr
                Console.WriteLine($"Exception: {ex.Message}");
                return View("Error");
            }
        }

        // GET: API'den sonucun alýnmasý
        [HttpGet]
        public async Task<IActionResult> GetResult()
        {
            // TempData'dan request_id'yi al
            string requestId = TempData["RequestId"]?.ToString();
            Console.WriteLine($"Request ID: {requestId}");

            if (string.IsNullOrEmpty(requestId))
            {
                Console.WriteLine("Error: request_id not found.");
                return View("Error");
            }
            // API URL
            string apiUrl = $"https://api.magicapi.dev/api/v1/magicapi/hair/predictions/{requestId}";
            //var requestMessage = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            //requestMessage.Headers.Add("x-magicapi-key", ApiKey);  // API anahtarýnýzý buraya yazýn
            //requestMessage.Headers.Add("request_id", requestId);  // request_id baþlýk olarak ekleniyor

            try
            {
                
                // Durum kontrolü
                var result = await CheckProcessingStatus(apiUrl, requestId);

                // Sonuç baþarýlýysa, modalda göster
                if (!string.IsNullOrEmpty(result))
                {
                    //string processedImageUrl = result.ToString();
                    return Redirect(result);
                }
                else
                {
                    Console.WriteLine("Processing failed or result is empty.");
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                // Hata durumu
                Console.WriteLine($"Exception: {ex.Message}");
                return View("Error");
            }
        }

        // Durum kontrol fonksiyonu
        private async Task<string> CheckProcessingStatus(string apiUrl, string requestId)
        {
            // API'den sonucu kontrol et
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            requestMessage.Headers.Add("x-magicapi-key", ApiKey);  // API anahtarýnýzý buraya yazýn
            requestMessage.Headers.Add("request_id", requestId);  // request_id baþlýk olarak ekleniyor

            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                dynamic json = JsonConvert.DeserializeObject(jsonResponse);
                string status = json?.status;
                string result = json?.result;

                Console.WriteLine($"Status: {status}");
                Console.WriteLine($"Result: {result}");

                if (status == "succeeded" && !string.IsNullOrEmpty(result))
                {
                    // Ýþlem baþarýlý ise sonucu döndürüyoruz
                    return result;
                }
                else if (status == "starting" || status == "processing")
                {
                    // Ýþlem "starting" durumunda ise 5 saniye bekle ve tekrar kontrol et
                    Console.WriteLine($"Status is {status}, retrying in 5 seconds...");
                    await Task.Delay(5000);  // 5 saniye bekle

                    // Durumu tekrar kontrol et
                    return await CheckProcessingStatus(apiUrl, requestId);
                }
                else
                {
                    // Baþka bir hata durumu varsa
                    Console.WriteLine($"Error: Status {status}, Result: {result}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Hata durumu
                Console.WriteLine($"Exception during status check: {ex.Message}");
                return null;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
