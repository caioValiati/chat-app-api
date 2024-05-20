using messageApp_backend.models;
using Microsoft.EntityFrameworkCore;

namespace messageApp_backend.Models
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options)
            : base(options) 
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
