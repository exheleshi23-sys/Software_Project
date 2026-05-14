using Microsoft.EntityFrameworkCore;
using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Case> Cases { get; set; }
        public DbSet<Suspect> Suspects { get; set; }
        public DbSet<Evidence> Evidence { get; set; }
        public DbSet<ForensicReport> ForensicReports { get; set; }
        public DbSet<CaseNote> CaseNotes { get; set; }
    }
}