
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Myapp.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Myapp.data;

namespace Myapp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserContext _context;

        public AccountController(ILogger<AccountController> logger, UserContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .SingleOrDefaultAsync(u => u.EmailId == email);

                if (user != null && VerifyPassword(password, user.Password))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.EmailId),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, user.Role) // Include the role in claims
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid credentials");
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(string email, string password)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingUser = await _context.Users
                        .SingleOrDefaultAsync(u => u.EmailId == email);

                    if (existingUser != null)
                    {
                        ModelState.AddModelError(string.Empty, "User already exists");
                        return View();
                    }

                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                    var newUser = new User
                    {
                        EmailId = email,
                        Password = hashedPassword, // Store the hashed password
                        Role = "User" // Set default role as 'User'
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while signing up a user.");
                ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Add AccessDenied action
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
