using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Myapp.data;
using Myapp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myapp.ViewModels;
namespace Myapp.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserContext _context;

        public UsersController(UserContext context)
        {
            _context = context;
        }

        // GET: api/v1/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/v1/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        // POST: api/v1/users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/v1/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, [FromBody] UserViewModel userViewModel)
        {
            if (userViewModel == null || id != userViewModel.Id)
            {
                return BadRequest(); // Return 400 Bad Request if the data is invalid
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound(); // Return 404 if the user is not found
            }

            existingUser.EmailId = userViewModel.EmailId; // Update EmailId
            // Update other fields as needed

            _context.Entry(existingUser).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound(); // Return 404 if the user was deleted by another request
                }
                else
                {
                    throw; // Rethrow if it's not a concurrency issue
                }
            }

            return NoContent(); // Return 204 No Content to indicate success
        }

        // DELETE: api/v1/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
