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
        private readonly AppDbContext _context; // Veritaban� ba�lam�
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
                    
                    // Tarih ve saati birle�tirerek randevu tarihi olu�tur
                    DateTime appointmentDateTime = selectedDate.Date.Add(TimeSpan.Parse(selectedHour));

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

        // POST: Kullan�c�dan gelen veriyi API'ye g�nder
        [HttpPost]
        public async Task<IActionResult> AnalyzeImage(string imageUrl, string editingType, string colorDescription, string hairstyleDescription)
        {
            // API URL
            string ApiUrl = "https://api.magicapi.dev/api/v1/magicapi/hair/hair";
            // API'ye g�nderilecek veri
            var requestData = new
            {
                image = imageUrl,
                editing_type = editingType,
                color_description = colorDescription,
                hairstyle_description = hairstyleDescription
            };
            try
            {
                // API'ye POST iste�i g�nder
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json")
                };

                // x-magicapi-key ve Content-Type ba�l�klar�n� ekle
                requestMessage.Headers.Add("x-magicapi-key", ApiKey);
                //requestMessage.Content.Headers.Add("Content-Type", "application/json");

                // API'ye istek g�nder
                var response = await _httpClient.SendAsync(requestMessage);

                // D�n�� verisini JSON olarak al
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Ba�ar�l� d�n��
                if (response.IsSuccessStatusCode)
                {
                    // API'den gelen yan�t� i�leme
                    dynamic json = JsonConvert.DeserializeObject(jsonResponse);
                    string requestId = json?.request_id;

                    // request_id de�erini konsola yazd�r
                    Console.WriteLine($"Request ID: {requestId}");

                    // request_id'yi TempData'ya ekleyelim
                    TempData["RequestId"] = requestId;

                    // Ba�ar�yla al�nan request_id de�erini GET metoduna g�nder
                    return RedirectToAction("GetResult");
                }
                else
                {
                    // Hata durumu: API'den d�nen hata mesaj�n� konsola yazd�r
                    Console.WriteLine($"API Error: {jsonResponse}");
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda hata mesaj�n� konsola yazd�r
                Console.WriteLine($"Exception: {ex.Message}");
                return View("Error");
            }
        }

        // GET: API'den sonucun al�nmas�
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
            //requestMessage.Headers.Add("x-magicapi-key", ApiKey);  // API anahtar�n�z� buraya yaz�n
            //requestMessage.Headers.Add("request_id", requestId);  // request_id ba�l�k olarak ekleniyor

            try
            {
                
                // Durum kontrol�
                var result = await CheckProcessingStatus(apiUrl, requestId);

                // Sonu� ba�ar�l�ysa, modalda g�ster
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
            requestMessage.Headers.Add("x-magicapi-key", ApiKey);  // API anahtar�n�z� buraya yaz�n
            requestMessage.Headers.Add("request_id", requestId);  // request_id ba�l�k olarak ekleniyor

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
                    // ��lem ba�ar�l� ise sonucu d�nd�r�yoruz
                    return result;
                }
                else if (status == "starting" || status == "processing")
                {
                    // ��lem "starting" durumunda ise 5 saniye bekle ve tekrar kontrol et
                    Console.WriteLine($"Status is {status}, retrying in 5 seconds...");
                    await Task.Delay(5000);  // 5 saniye bekle

                    // Durumu tekrar kontrol et
                    return await CheckProcessingStatus(apiUrl, requestId);
                }
                else
                {
                    // Ba�ka bir hata durumu varsa
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
