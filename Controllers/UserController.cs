using Hairdresser.Models;
using Hairdresser.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Hairdresser.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context; // Veritabanı bağlamı

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Veritabanındaki tüm kullanıcıları al
            var users = await _context.User.ToListAsync();
            return View(users);  // Kullanıcıları View'a gönder
        }   

        public async Task<IActionResult> Appointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Giriş yapan kullanıcının ID'si
                                                                         // Veritabanından oturumdaki kullanıcıya ait randevuları getir
            var userAppointments = await  _context.Appointments
                                    .Where(a => a.AppointmentUserID == userId)
                                    .Include(a => a.Service)
                                    .Include(a => a.Employee)
                                    .ToListAsync();

            return View(userAppointments); // View'e gönder
        }

        public async Task<IActionResult> EditAppointment()
        {
            return RedirectToAction("Appointments");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.User.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                try
                {
                    model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    _context.User.Add(model);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // Hata loglama işlemi
                    Console.Error.WriteLine($"Registration Error: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while processing your request.");
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model Error: {error.ErrorMessage}");
                }
                return View(model);
            }
            // Eğer ModelState geçerli değilse, form tekrar gösterilecek
            return View(model);

        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // E-posta ile kullanıcıyı veritabanında arayın
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null) // Kullanıcı bulunduysa
            {
                if (BCrypt.Net.BCrypt.Verify(password, user.Password)) // Şifre doğru mu?
                {

                    // Kullanıcı giriş bilgileri için bir claim listesi oluştur
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString())
                    };

                    // ClaimsPrincipal oluştur
                    var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Oturumu başlat
                    await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

                    // Şifre doğru, Home Index'e yönlendir
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Şifre yanlış, hata mesajı ekle
                    ModelState.AddModelError("", "Incorrect password. Please try again.");
                }
            }
            else
            {
                // Kullanıcı bulunamadı, hata mesajı ekle
                ModelState.AddModelError("", "No user found with the given email.");
            }

            // Hata durumunda Login görünümünü yeniden göster
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }


        // Profil sayfasını görüntüler
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Giriş yapan kullanıcının ID'si
            var user = await _context.User.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Profil güncelleme sayfasını görüntüler
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Giriş yapan kullanıcının ID'si
            var user = await _context.User.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Profil bilgilerini günceller
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(User model, string Password)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Giriş yapan kullanıcının ID'si
            var user = await _context.User.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Şifreyi kontrol et
            if (!BCrypt.Net.BCrypt.Verify(Password, user.Password))
            {
                TempData["ErrorMessage"] = "Incorrect password. Please try again.";
                return RedirectToAction("EditProfile");
            }

            // Şifre doğruysa bilgileri güncelle
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult EditPassword()
        {
            return View();
        }

        // Şifreyi günceller
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(string oldPassword, string newPassword)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Giriş yapan kullanıcının ID'si
            var user = await _context.User.FindAsync(userId);

            // Eski şifreyi kontrol et
            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
            {
                ModelState.AddModelError("OldPassword", "The provided password is incorrect.");
                return View("Profile");
            }

            // Yeni şifreyi hashleyip veritabanına kaydet
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password updated successfully.";
            return RedirectToAction("Profile");
        }
    }
}
