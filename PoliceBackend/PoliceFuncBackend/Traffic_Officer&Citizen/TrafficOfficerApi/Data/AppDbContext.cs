using Microsoft.EntityFrameworkCore;
using TrafficOfficerApi.Models;

namespace TrafficOfficerApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Fine> Fines { get; set; } = null!;
    public DbSet<Accident> Accidents { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!; 
    public DbSet<CitizenReport> CitizenReports { get; set; } = null!;
    
    
}