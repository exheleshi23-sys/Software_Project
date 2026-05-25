using Microsoft.EntityFrameworkCore;
using CrimeMapApi.Models;

namespace CrimeMapApi.Data
{
public class LocalDbContext : DbContext
{
public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options)
{
}

public DbSet<CrimeRecord> CrimeRecords { get; set; } = null!;
public DbSet<CrimeData> CrimeData { get; set; } = null!;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
base.OnModelCreating(modelBuilder);

modelBuilder.Entity<CrimeData>(entity =>
{
entity.HasNoKey();
entity.ToTable("crime_data");
});
}
}
}
