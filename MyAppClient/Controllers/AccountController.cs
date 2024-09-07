using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MyAppClient.Models;
using MyAppClient.data;
using System.Security.Claims;
using System.Text;
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
                // Ensure the email and password are not empty
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError(string.Empty, "Email and password are required.");
                    return View();
                }

                using (var client = new HttpClient())
                {
                    // Send a request to the external API to create a new user
                    var request = new HttpRequestMessage(HttpMethod.Post, "http://myappapi:5211/api/v1/auth/signup")
                    {
                        Content = new StringContent(
                            JsonConvert.SerializeObject(new { email, password }),
                            Encoding.UTF8,
                            "application/json")
                    };

                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        // Optionally, handle successful signup (e.g., redirect to login page)
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        // Read and display error message from the API response
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, $"Error: {errorContent}");
                    }
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
                    var request = new HttpRequestMessage(HttpMethod.Post, "http://myappapi:5211/api/v1/auth/login")
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
                    }
                }
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



