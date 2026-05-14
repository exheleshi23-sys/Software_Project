using Microsoft.EntityFrameworkCore;
using TrafficOfficerApi.Data;
using TrafficOfficerApi.Models;

namespace TrafficOfficerApi.Services;

public class CitizenService : ICitizenService
{
    private readonly AppDbContext _context;

    public CitizenService(AppDbContext context)
    {
        _context = context;
    }

    // List all reports created by this citizen
    public async Task<IEnumerable<CitizenReport>> GetMyReportsAsync(string citizenId)
    {
        return await _context.CitizenReports
            .Where(r => r.CitizenId == citizenId)
            .ToListAsync();
    }

    // Create a new report
    public async Task<CitizenReport> CreateReportAsync(CitizenReport report)
    {
        _context.CitizenReports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    // List all fines belonging to this citizen
    public async Task<IEnumerable<Fine>> GetMyFinesAsync(string citizenId)
    {
        // Fix for CS0019: Convert the string ID from the token to an int 
        // to match the Fine.UserId property in the database.
        if (int.TryParse(citizenId, out int idAsInt))
        {
            return await _context.Fines
                .Where(f => f.UserId == idAsInt)
                .ToListAsync();
        }

        return new List<Fine>();
    }

    // Process a fine payment
    public async Task<bool> PayFineAsync(int fineId)
    {
        var fine = await _context.Fines.FindAsync(fineId);
        if (fine == null) return false;

        // Update the status string to match your Fine.cs [Column("FineStatus")]
        fine.FineStatus = "Paid";
        
        await _context.SaveChangesAsync();
        return true;
    }
}