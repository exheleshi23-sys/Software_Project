using TrafficOfficerApi.Models; // Add this!
using TrafficOfficerApi.Data;
using Microsoft.EntityFrameworkCore;

namespace TrafficOfficerApi.Services;

public class VehicleService : IVehicleService
{
    private readonly AppDbContext _context;

    public VehicleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Vehicle?> LookupVehicleAsync(string plate)
    {
        // This logic searches the database for a matching plate
        return await _context.Vehicles
            .FirstOrDefaultAsync(v => v.PlateNumber == plate);
    }
}