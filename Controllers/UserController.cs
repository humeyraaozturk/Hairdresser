using Hairdresser.Models;
using Hairdresser.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using BCrypt.Net;
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

        public IActionResult AddUser()
        {
            var user = new User
            {
                FullName = "John Doe",
                Email = "john.doe@example.com",
                Password = "securepassword"
            };

            _context.User.Add(user);
            _context.SaveChanges();

            return Content("User added successfully!");
        }

        public IActionResult Appointments()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        public IActionResult Profile()
        {
            return View();
        }    

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User model)
        {
            if (ModelState.IsValid)  // Form geçerli mi kontrol et
            {
                // Benzersiz E-posta kontrolü
                if (await _context.User.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                try
                {
                    // Şifreyi hash'le
                    model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

                    // Veritabanına kullanıcıyı ekle
                    _context.User.Add(model);
                    await _context.SaveChangesAsync();

                    // Kayıt başarılı, yönlendir
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // Hata durumunda ModelState'e hata ekle
                    ModelState.AddModelError("", $"An error occurred while registering: {ex.Message}");
                }
            }
            else
            {
                // Hata varsa, hata mesajlarını kontrol et
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);  // Hata mesajlarını konsola yazdır
                }
            }

            // Eğer ModelState geçerli değilse, form tekrar gösterilecek
            return View(model);

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
                        new Claim(ClaimTypes.NameIdentifier, user.ID.ToString())
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
    }
}
