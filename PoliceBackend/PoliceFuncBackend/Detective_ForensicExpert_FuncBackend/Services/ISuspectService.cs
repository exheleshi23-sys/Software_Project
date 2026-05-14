using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public interface ISuspectService
    {
        Task<List<Suspect>> GetAllSuspectsAsync();
        Task<Suspect?> GetSuspectByIdAsync(int id);
        Task<Suspect> CreateSuspectAsync(CreateSuspectDto dto);
        Task<Suspect?> UpdateSuspectAsync(int id, UpdateSuspectDto dto);
        Task<bool> DeleteSuspectAsync(int id);
    }
}