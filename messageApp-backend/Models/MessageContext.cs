using messageApp_backend.models;
using Microsoft.EntityFrameworkCore;

namespace messageApp_backend.Models
{
    public class MessageContext : DbContext
    {
        public MessageContext(DbContextOptions<MessageContext> options)
            : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Message>().Property(e => e.Id).ValueGeneratedNever();
        }
    }
}
