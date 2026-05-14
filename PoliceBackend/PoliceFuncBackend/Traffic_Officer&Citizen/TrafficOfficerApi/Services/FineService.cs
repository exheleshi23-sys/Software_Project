using Microsoft.EntityFrameworkCore;
using TrafficOfficerApi.Data;
using TrafficOfficerApi.DTOs;
using TrafficOfficerApi.Models;

namespace TrafficOfficerApi.Services;

public class FineService : IFineService
{
    private readonly AppDbContext _context;

    public FineService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FineDto>> GetFinesAsync(DateTime? date, string? status, string? plate)
    {
        var query = _context.Fines.AsQueryable();

        if (date.HasValue)
            query = query.Where(f => f.IssueDate.Date == date.Value.Date);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(f => f.FineStatus == status);

        // Note: The traffic_fine table in your SQL doesn't have a plate column directly, 
        // it links via Violation_ID. For now, we skip plate filtering or link to road_violation.
        
        return await query.Select(f => new FineDto
        {
            Id = f.Id,
            Amount = f.Amount,
            DateIssued = f.IssueDate,
            Status = f.FineStatus,
            LicensePlate = "Lookup Required", // Placeholder since it's not in this table
            OffenseType = "Violation ID: " + f.ViolationId
        }).ToListAsync();
    }

    public async Task<FineDto?> GetFineByIdAsync(int id)
    {
        var f = await _context.Fines.FindAsync(id);
        if (f == null) return null;

        return new FineDto
        {
            Id = f.Id,
            Amount = f.Amount,
            DateIssued = f.IssueDate,
            Status = f.FineStatus,
            LicensePlate = "Lookup Required"
        };
    }

    public async Task<FineDto> CreateFineAsync(CreateFineDto dto, string officerId)
    {
        var fine = new Fine
        {
            Amount = (int)dto.Amount, // Explicit cast from decimal to int to match SQL
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            FineStatus = "Pending",
            UserId = int.TryParse(officerId, out int id) ? id : 1, // Matches User_ID
            ViolationId = 1 // Default or lookup logic required for Violation_ID
        };

        _context.Fines.Add(fine);
        await _context.SaveChangesAsync();

        return new FineDto
        {
            Id = fine.Id,
            Amount = fine.Amount,
            DateIssued = fine.IssueDate,
            Status = fine.FineStatus,
            LicensePlate = dto.LicensePlate
        };
    }

    public async Task<bool> CancelFineAsync(int id)
    {
        var fine = await _context.Fines.FindAsync(id);
        if (fine == null) return false;

        fine.FineStatus = "Cancelled";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<object> GetFinesStatsByTypeAsync()
    {
        return await _context.Fines
            .GroupBy(f => f.ViolationId)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();
    }

    public async Task<object> GetDailyFinesStatsAsync()
    {
        return await _context.Fines
            .GroupBy(f => f.IssueDate.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();
    }
}