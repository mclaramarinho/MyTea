using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyTeaApp.Models;

namespace MyTeaApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
       
        public DbSet<Department> Department { get; set; } 
        public DbSet<Record> Records { get; set; } = default!;
        public DbSet<WBS> WBS { get; set; } = default!;
        public DbSet<RecordFraction> RecordFraction { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
          
        }

    }
}
