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
            var userAppointments = await _context.Appointments
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

        [HttpGet]
        public IActionResult Register()
        {
            // Roller ViewBag'e aktarılıyor
            var roles = _context.Roles.ToList();
            // Eğer "user" rolü veritabanında yoksa ekle
            if (!roles.Any(r => r.RoleName == "user"))
            {
                roles.Add(new Role { RoleID = 1, RoleName = "user" });  // Varsayılan "user" rolü ekleyin
            }
            ViewBag.Roles = roles;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string UserID, string FullName, string Email, string PhoneNumber, string Password, int UserRoleID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kullanıcı emaili kontrol et, eğer zaten varsa hata mesajı göster
                    var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Email == Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "This email is already registered.");
                        return View();
                    }

                    // Şifreyi hashle
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);

                    // Yeni kullanıcı oluştur
                    var user = new User
                    {
                        UserID = UserID,
                        FullName = FullName,
                        Email = Email,
                        PhoneNumber = PhoneNumber,
                        Password = hashedPassword,
                        UserRoleID = UserRoleID // Kullanıcının seçtiği rol
                    };

                    // Kullanıcıyı veritabanına ekle
                    _context.User.Add(user);
                    await _context.SaveChangesAsync();

                    // Başarılı mesajı göster
                    TempData["SuccessMessage"] = "Registration successful!";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // Hata mesajı ekle
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                }
            }
            else
            {
                // Model geçerli değilse hata mesajlarını yazdır
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model Error: {error.ErrorMessage}");
                }
            }

            // Rol listesini tekrar ViewBag'e aktar ve formu yeniden göster
            var roles = await _context.Roles.ToListAsync();
            ViewBag.Roles = roles;
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
       
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            // Kullanıcıyı veritabanında e-posta ile arayın
            var user = await _context.User
                                     .Include(u => u.Role) // Rol bilgisiyle birlikte kullanıcıyı al
                                     .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // Kullanıcı bulunamadı
                ModelState.AddModelError("", "No user found with the given email.");
                return View();
            }

            // Şifre doğrulaması
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ModelState.AddModelError("", "Incorrect password. Please try again.");
                return View();
            }

            // Kullanıcı giriş bilgileri için bir claim listesi oluştur
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Role, user.Role.RoleName) // Rol ismini kullan
            };

            // ClaimsPrincipal oluştur
            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Oturumu başlat
            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

            // Kullanıcı ID'sini session'a ekleyin (isteğe bağlı)
            HttpContext.Session.SetString("UserID", user.UserID.ToString());       

            // Varsayılan yönlendirme
            return RedirectToAction("Index", "Home");
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
