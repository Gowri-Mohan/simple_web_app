using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAppClient.Models;
namespace MyAppClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Authorize]
        public IActionResult Index()
        {
            // Get the username from the claims
            var username = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.Name) : "Guest";

            // Pass the username to the view using ViewBag
            ViewBag.Username = username;

            var token = Request.Cookies["AuthToken"];

            ViewBag.Token = token;

            // The view will handle loading the users via jQuery and AJAX
            return View();
        }

        [Authorize(Policy = "RequireAdminRole")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
