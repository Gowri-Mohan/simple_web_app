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
using Newtonsoft.Json;

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
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7057/api/v1/auth/login")
                    {
                        Content = new StringContent(
                            JsonConvert.SerializeObject(new { username, password }),
                            Encoding.UTF8,
                            "application/json")
                    };

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(content);

                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token))
                    {
                        // Store JWT token in cookie
                        Response.Cookies.Append("AuthToken", tokenResponse.Token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true, // Ensure this is true in production
                            SameSite = SameSiteMode.Strict
                        });

                       // Set authentication cookie manually
                        var claims = new[] { new Claim(ClaimTypes.Name, username) };
                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                        };


                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new 
                        ClaimsPrincipal(claimsIdentity), authProperties);

                        // Redirect to home page
                            return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");
                ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                return View();
            }
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



