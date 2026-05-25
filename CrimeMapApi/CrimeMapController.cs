using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrimeMapApi.Data;
using CrimeMapApi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace CrimeMapApi.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class CrimeMapController : ControllerBase
{
private readonly LocalDbContext _context;

public CrimeMapController(LocalDbContext context)
{
_context = context;
}

[HttpGet("thermal-blueprint")]
public async Task<IActionResult> GetThermalMapData()
{
// Fetch raw rows from Postgres using explicit, quoted column mappings
var rawRows = new List<dynamic>();

using (var command = _context.Database.GetDbConnection().CreateCommand())
{
// Explicitly pull the target columns with their exact database casings and plurals
command.CommandText = "SELECT \"Location\", \"Street Lighting\", \"Community Patrols\" FROM crime_data";
await _context.Database.OpenConnectionAsync();

using (var reader = await command.ExecuteReaderAsync())
{
while (await reader.ReadAsync())
{
rawRows.Add(new
{
Location = reader["Location"]?.ToString() ?? "Unknown",
StreetLighting = reader["Street Lighting"] != DBNull.Value ? Convert.ToDouble(reader["Street Lighting"]) : 0.0,
CommunityPatrols = reader["Community Patrols"] != DBNull.Value ? Convert.ToDouble(reader["Community Patrols"]) : 0.0
});
}
}
}

// Group records by neighborhood location name
var groupedData = rawRows
.GroupBy(c => c.Location)
.Select(g => new
{
Location = g.Key,
TotalCrimes = g.Count(),
AvgLighting = g.Average(c => (double)c.StreetLighting),
AvgPatrols = g.Average(c => (double)c.CommunityPatrols)
})
.ToList();

// Process the thermal intensity calculations
var thermalBlueprint = groupedData.Select(data =>
{
string risk = "Low";
if (data.TotalCrimes > 15 || (data.TotalCrimes > 10 && data.AvgLighting < 4))
{
risk = "High";
}
else if (data.TotalCrimes > 5)
{
risk = "Medium";
}

return new
{
data.Location,
data.TotalCrimes,
AvgLighting = Math.Round(data.AvgLighting, 2),
AvgPatrols = Math.Round(data.AvgPatrols, 2),
RiskAssessment = risk
};
}).ToList();

return Ok(thermalBlueprint);
}
}
}