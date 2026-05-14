using Microsoft.EntityFrameworkCore;
using TrafficOfficerApi.Data;
using TrafficOfficerApi.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. REGISTER SERVICES ---

// Add Controllers and make Enums readable (e.g., "High" instead of "2")
builder.Services.AddControllers()
    .AddJsonOptions(options => 
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// MariaDB Connection Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Dependency Injection for your Business Logic
builder.Services.AddScoped<IAccidentService, AccidentService>();
builder.Services.AddScoped<IFineService, FineService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<ICitizenService, CitizenService>();

// Swagger Generation Setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Traffic Officer API", 
        Version = "v1",
        Description = "API for Traffic Accidents and Fines" 
    });
});

var app = builder.Build();

// --- 2. CONFIGURE MIDDLEWARE PIPELINE ---

// Enable Swagger UI (Ensures it works at /swagger)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Traffic Officer API V1");
        c.RoutePrefix = "swagger"; 
    });
}

// Redirect HTTP to HTTPS (Local dev might warn about this, which is fine)
app.UseHttpsRedirection();

// IMPORTANT: Order matters here for Security
app.UseAuthentication(); 
app.UseAuthorization();

// This connects your [Route("api/[controller]")] attributes to the web server
app.MapControllers();

app.Run();