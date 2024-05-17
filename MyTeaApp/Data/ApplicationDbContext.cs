using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyTeaApp.Models;

namespace MyTeaApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
       
        public DbSet<MyTeaApp.Models.Department> Department { get; set; } 
        public DbSet<MyTeaApp.Models.Record> Record { get; set; } = default!;
        public DbSet<MyTeaApp.Models.WBS> WBS { get; set; } = default!;
        public DbSet<MyTeaApp.Models.RecordFraction> RecordFraction { get; set; } = default!;
    }
}
