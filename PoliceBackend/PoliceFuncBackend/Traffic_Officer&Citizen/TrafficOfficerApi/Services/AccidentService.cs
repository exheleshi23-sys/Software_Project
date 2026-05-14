using Microsoft.EntityFrameworkCore;
using TrafficOfficerApi.Data;
using TrafficOfficerApi.DTOs;
using TrafficOfficerApi.Models;

namespace TrafficOfficerApi.Services;

public class AccidentService : IAccidentService
{
    private readonly AppDbContext _context;

    public AccidentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AccidentDto>> GetAccidentsAsync()
    {
        // Mapping the database model (Accident) to the data transfer object (AccidentDto)
        return await _context.Accidents
            .Select(a => new AccidentDto
            {
                Id = a.Id,
                DateReported = a.AccidentDate,
                Location = a.Location,
                Description = a.Description,
                Severity = a.Severity.ToString(),
                InvolvedPlates = "See Description" // InvolvedPlates is not a column in your SQL, pulling from description or dummy
            })
            .ToListAsync();
    }

    public async Task<AccidentDto?> GetAccidentByIdAsync(int id)
    {
        var a = await _context.Accidents.FindAsync(id);
        
        if (a == null) return null;

        return new AccidentDto
        {
            Id = a.Id,
            DateReported = a.AccidentDate,
            Location = a.Location,
            Description = a.Description,
            Severity = a.Severity.ToString(),
            InvolvedPlates = "See Description"
        };
    }

    public async Task<AccidentDto> CreateAccidentAsync(CreateAccidentDto createDto, string officerId)
    {
        // Create the model object to save to the database
        var accident = new Accident
        {
            Location = createDto.Location,
            Description = createDto.Description,
            Severity = (int)createDto.Severity, // Casting the Enum to the int used in your SQL
            AccidentDate = DateTime.UtcNow,
            AccidentTime = DateTime.Now.ToString("HH:mm"), // SQL table has a varchar(20) AccidentTime
            Status = "Open",
            UserId = int.TryParse(officerId, out int id) ? id : 1 // Mapping to the User_ID in your SQL
        };

        _context.Accidents.Add(accident);
        await _context.SaveChangesAsync();

        // Return the DTO version of the newly created accident
        return new AccidentDto
        {
            Id = accident.Id,
            DateReported = accident.AccidentDate,
            Location = accident.Location,
            Description = accident.Description,
            Severity = accident.Severity.ToString(),
            InvolvedPlates = createDto.InvolvedPlates
        };
    }

    public async Task<bool> UpdateAccidentAsync(int id, UpdateAccidentDto updateDto)
    {
        var accident = await _context.Accidents.FindAsync(id);
        
        if (accident == null) return false;

        accident.Description = updateDto.Description;
        accident.Severity = (int)updateDto.Severity;

        _context.Entry(accident).State = EntityState.Modified;
        
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }
}