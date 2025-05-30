using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kutuphane.Data;
using Kutuphane.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Kutuphane.Controllers
{
    public class UserController : Controller
    {
        private readonly KutuphaneDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(KutuphaneDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Seed initial user if no users exist
        private async Task SeedInitialUserIfNeeded()
        {
            if (!_context.Users.Any())
            {
                var initialUser = new User
                {
                    Username = "admin",
                    Email = "admin@kutuphane.com",
                    Name = "Sistem",
                    Surname = "Yöneticisi",
                    IsAdmin = true,
                    PasswordHash = HashPassword("admin123")
                };

                _context.Users.Add(initialUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Initial admin user created");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            await SeedInitialUserIfNeeded();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            _logger.LogInformation($"Login attempt for username: {username}");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Kullanıcı adı ve şifre boş bırakılamaz.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
            {
                _logger.LogWarning($"Login failed: User not found - {username}");
                ModelState.AddModelError("", "Kullanıcı bulunamadı. Lütfen kayıt olun.");
                return View();
            }

            var inputHash = HashPassword(password);
            
            if (!VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning($"Login failed: Invalid password for user - {username}");
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Yönetici" : "Kullanıcı")
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                });

            _logger.LogInformation($"User logged in successfully: {username}");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            _logger.LogInformation($"Registration attempt for username: {model.Username}");

            // Validate model first
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try 
            {
                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    _logger.LogWarning($"Registration failed: Username already exists - {model.Username}");
                    ModelState.AddModelError("Username", "Bu kullanıcı adı zaten kullanılmaktadır.");
                    return View(model);
                }

                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    _logger.LogWarning($"Registration failed: Email already exists - {model.Email}");
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılmaktadır.");
                    return View(model);
                }

                // Validate password
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.AddModelError("Password", "Şifre boş bırakılamaz.");
                    return View(model);
                }

                // Hash the password
                model.PasswordHash = HashPassword(model.Password);
                
                // First user is admin
                model.IsAdmin = !_context.Users.Any();

                // Add the new user
                _context.Users.Add(model);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User registered successfully: {model.Username}");

                // Redirect to login page with a success message
                TempData["SuccessMessage"] = "Hesabınız başarıyla oluşturuldu. Giriş yapabilirsiniz.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering user: {model.Username}");
                ModelState.AddModelError("", "Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyin.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            if (!User.IsInRole("Yönetici"))
            {
                return Forbid();
            }

            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(int id, User updatedUser)
        {
            if (!User.IsInRole("Yönetici"))
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Name = updatedUser.Name;
            user.Surname = updatedUser.Surname;
            user.Email = updatedUser.Email;
            user.IsAdmin = updatedUser.IsAdmin;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profil()
        {
            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }
    }
} 