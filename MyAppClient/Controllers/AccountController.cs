using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using MyAppClient.Models;
using MyAppClient.data;
using System.Threading.Tasks;
using System.Linq;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics;

namespace MyAppClient.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(ILogger<AccountController> logger, UserContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
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
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                //HttpClient client =new()

                // Redirect to home page or another secure page
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");
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

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}


