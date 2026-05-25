using Microsoft.EntityFrameworkCore;
using CrimeMapApi.Data;
using CrimeMapApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 1. ADD THIS LINE RIGHT HERE to force lowercase routing:
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.AddDbContext<LocalDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<LocalDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1. REGISTER THE CORS SERVICE HERE
builder.Services.AddCors(options =>
{
options.AddDefaultPolicy(policy =>
{
policy.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader();
});
});

var app = builder.Build();

// 2. ACTIVATE CORS MIDDLEWARE HERE
app.UseCors();

app.MapControllers();


app.Run();