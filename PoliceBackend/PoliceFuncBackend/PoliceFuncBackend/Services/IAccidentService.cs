using PoliceFuncBackend.DTOs;

namespace PoliceFuncBackend.Services
{
    public interface IAccidentService
    {
        Task<IEnumerable<AccidentDto>> GetAccidentsAsync();

        Task<AccidentDto?> GetAccidentByIdAsync(int id);

        Task<AccidentDto> CreateAccidentAsync(
            CreateAccidentDto dto,
            string officerId
        );

        Task<bool> UpdateAccidentAsync(
            int id,
            UpdateAccidentDto dto
        );
    }
}