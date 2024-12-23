using Hairdresser.Data;
using Hairdresser.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hairdresser.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context; // Veritabanı bağlamı
        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Bekleyen randevuları listelemek için veri çekiyoruz.
            var appointments = await _context.Appointments
                .Include(a => a.User) // Kullanıcı bilgilerini dahil ediyoruz.
                .Include(a => a.Service) // Hizmet bilgilerini dahil ediyoruz.
                .Where(a => a.Status == "Pending") // Sadece "Beklemede" durumundaki randevuları çekiyoruz.
                .ToListAsync();

            var employees = await _context.Employees.ToListAsync();

            ViewBag.Employees = employees;

            // Çalışma saatleri
            var availableHours = new List<string>
            {
                "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00"
            };
            ViewBag.AvailableHours = availableHours;

            var services = await _context.Services.ToListAsync();
            ViewBag.Services = services;

            // Randevuları View'a gönderiyoruz.
            return View(appointments);
        }

        // Çalışan Ekleme Sayfası
        public IActionResult AddEmployee()
        {          
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee(string employeeId, string fullName, List<string> availableHours, int employeeServiceID)
        {
            if (string.IsNullOrEmpty(fullName) || availableHours == null || !availableHours.Any() || employeeServiceID <= 0)
            {
                TempData["ErrorMessage"] = "Tüm alanları doldurmanız gerekiyor.";
                return RedirectToAction("Dashboard");
            }

            var service = await _context.Services.FindAsync(employeeServiceID);
            if (service == null)
            {
                TempData["ErrorMessage"] = "Geçersiz uzmanlık alanı.";
                return RedirectToAction("Dashboard");
            }

            // Seçilen saatleri JSON formatına dönüştür
            string availableHoursString = string.Join(",", availableHours);

            // Yeni çalışan objesi oluştur
            var employee = new Employee
            {
                EmployeeID = employeeId,
                FullName = fullName,
                AvailableHours = availableHoursString, // Seçilen saatler
                EmployeeServiceID = employeeServiceID,
                Service = service
            };

            try
            {
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Çalışan başarıyla eklendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveEmployee(string employeeId)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeID == employeeId);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Çalışan bulunamadı.";
                return RedirectToAction(nameof(Dashboard));
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Çalışan başarıyla silindi.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId, string newStatus)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found!";
            }

            // Randevunun durumunu güncelliyoruz.
            appointment.Status = newStatus;
            await _context.SaveChangesAsync();

            // İşlem sonrası Dashboard'a yönlendiriyoruz.
            return RedirectToAction("Dashboard","Admin");
        }

    }
}
