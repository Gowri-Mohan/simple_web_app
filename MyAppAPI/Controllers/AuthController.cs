/*
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MyAppAPI.data;
using Microsoft.EntityFrameworkCore;

namespace MyAppAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserContext _userContext;
        private readonly IConfiguration _configuration;

        public AuthController(UserContext userContext, IConfiguration configuration)
        {
            _userContext = userContext;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Extract username and password
            string username = request.Username;
            string password = request.Password;

            // Find user in the database
            var user = await _userContext.Users
                .SingleOrDefaultAsync(u => u.EmailId.ToLower() == username.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            // Create claims for JWT
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Generate JWT token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Return JWT token
            return Ok(new { Token = tokenString });
        }
    }

    // Define a model to bind the request body
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
*/

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MyAppAPI.data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using MyAppAPI.Models;

namespace MyAppAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserContext _userContext;
        private readonly IConfiguration _configuration;

        public AuthController(UserContext userContext, IConfiguration configuration)
        {
            _userContext = userContext;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Extract username and password
            string username = request.Username;
            string password = request.Password;

            // Find user in the database
            var user = await _userContext.Users
                .SingleOrDefaultAsync(u => u.EmailId.ToLower() == username.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            // Create claims for JWT
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Generate JWT token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Return JWT token
            return Ok(new { Token = tokenString });
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and password are required.");
            }

            // Check if the user already exists
            var existingUser = await _userContext.Users
                .SingleOrDefaultAsync(u => u.EmailId.ToLower() == request.Email.ToLower());

            if (existingUser != null)
            {
                return Conflict("User already exists.");
            }

            // Hash the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create a new user
            var newUser = new User
            {
                EmailId = request.Email,
                Password = hashedPassword,
                Role = "User" // Default role
            };

            // Add the new user to the database
            _userContext.Users.Add(newUser);
            await _userContext.SaveChangesAsync();

            return Ok("User registered successfully.");
        }
    }

    // Define models for request binding
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SignUpRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}

