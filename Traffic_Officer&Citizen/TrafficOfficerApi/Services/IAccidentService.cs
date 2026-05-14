using TrafficOfficerApi.DTOs;

namespace TrafficOfficerApi.Services;

public interface IAccidentService
{
    Task<IEnumerable<AccidentDto>> GetAccidentsAsync();
    Task<AccidentDto?> GetAccidentByIdAsync(int id);
    Task<AccidentDto> CreateAccidentAsync(CreateAccidentDto createDto, string officerId);
    Task<bool> UpdateAccidentAsync(int id, UpdateAccidentDto updateDto);
}
