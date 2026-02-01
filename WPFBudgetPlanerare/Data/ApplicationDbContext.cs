using Microsoft.EntityFrameworkCore;
using WPFBudgetPlanerare.Models;

namespace WPFBudgetPlanerare.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }

        public ApplicationDbContext()
        {
        }
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=WPFBudgetPlanerareDb;Trusted_Connection=True;MultipleActiveResultSets=true");
            }
        }
    }
}
