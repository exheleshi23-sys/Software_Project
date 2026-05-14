using Microsoft.EntityFrameworkCore;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public class SuspectService : ISuspectService
    {
        private readonly ApplicationDbContext _context;

        public SuspectService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Suspect>> GetAllSuspectsAsync()
        {
            return await _context.Suspects.ToListAsync();
        }

        public async Task<Suspect?> GetSuspectByIdAsync(int id)
        {
            return await _context.Suspects.FindAsync(id);
        }

        public async Task<Suspect> CreateSuspectAsync(CreateSuspectDto dto)
        {
            var suspect = new Suspect
            {
                Evidence = dto.Evidence,
                InvestigationId = dto.InvestigationId
            };

            _context.Suspects.Add(suspect);
            await _context.SaveChangesAsync();

            return suspect;
        }

        public async Task<Suspect?> UpdateSuspectAsync(int id, UpdateSuspectDto dto)
        {
            var suspect = await _context.Suspects.FindAsync(id);

            if (suspect == null)
            {
                return null;
            }

            suspect.Evidence = dto.Evidence;
            suspect.InvestigationId = dto.InvestigationId;

            await _context.SaveChangesAsync();

            return suspect;
        }

        public async Task<bool> DeleteSuspectAsync(int id)
        {
            var suspect = await _context.Suspects.FindAsync(id);

            if (suspect == null)
            {
                return false;
            }

            _context.Suspects.Remove(suspect);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}