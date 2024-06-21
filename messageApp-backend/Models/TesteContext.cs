using messageApp_backend.models;
using Microsoft.EntityFrameworkCore;

namespace messageApp_backend.Models
{
    public class TesteContext : DbContext
    {
        public TesteContext(DbContextOptions<TesteContext> options)
            : base(options)
        {
        }

        public DbSet<Teste> Testes { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Teste>()
                .HasOne(m => m.User)
                .WithMany(u => u.Testes)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
