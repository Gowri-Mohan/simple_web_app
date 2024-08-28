using Microsoft.EntityFrameworkCore;
using MyAppClient.Models;
namespace MyAppClient.data
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        private readonly IConfiguration _config;

        public UserContext(
            DbContextOptions<UserContext> options, 
            IConfiguration config)
            : base(options)
        {
            _config = config;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            optionsBuilder.UseNpgsql(_config.GetConnectionString("DefaultConnection"));
        }
    }
}

