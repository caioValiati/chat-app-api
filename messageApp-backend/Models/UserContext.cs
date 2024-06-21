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
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Property(e => e.Id).ValueGeneratedNever();

            // Dados iniciais
            modelBuilder.Entity<User>().HasData(
                new User {
                    Id = 1, 
                    UserName = "teste", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("teste"),
                    IsActive = true,
                },
                new User {
                    Id = 2, 
                    UserName = "aba", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("aba"),
                    IsActive = true,
                }
            );

        }
    }
}
