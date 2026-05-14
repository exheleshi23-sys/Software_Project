using Microsoft.EntityFrameworkCore;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public class EvidenceService : IEvidenceService
    {
        private readonly ApplicationDbContext _context;

        public EvidenceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Evidence>> GetEvidenceByCaseIdAsync(int caseId)
        {
            return await _context.Evidence
                .Where(e => e.CaseId == caseId)
                .ToListAsync();
        }

        public async Task<List<Evidence>> GetEvidenceQueueAsync()
        {
            return await _context.Evidence
                .Where(e => e.Status == "collected" || e.Status == "in_analysis")
                .ToListAsync();
        }

        public async Task<Evidence?> AssignAnalystAsync(int id, AssignAnalystDto dto)
        {
            var evidence = await _context.Evidence.FindAsync(id);

            if (evidence == null)
            {
                return null;
            }

            evidence.AnalyzedBy = dto.AnalystId;
            evidence.Status = "in_analysis";

            await _context.SaveChangesAsync();
            return evidence;
        }

        public async Task<Evidence?> UpdateStatusAsync(int id, UpdateEvidenceStatusDto dto)
        {
            var evidence = await _context.Evidence.FindAsync(id);

            if (evidence == null)
            {
                return null;
            }

            evidence.Status = dto.Status;

            await _context.SaveChangesAsync();
            return evidence;
        }

        public async Task<string?> GetChainOfCustodyAsync(int id)
        {
            var evidence = await _context.Evidence.FindAsync(id);

            if (evidence == null)
            {
                return null;
            }

            return evidence.ChainOfCustody;
        }

        public async Task<Evidence?> AddChainOfCustodyAsync(int id, ChainOfCustodyDto dto)
        {
            var evidence = await _context.Evidence.FindAsync(id);

            if (evidence == null)
            {
                return null;
            }

            var newEntry = $"{DateTime.Now}: {dto.Entry}";

            if (string.IsNullOrWhiteSpace(evidence.ChainOfCustody))
            {
                evidence.ChainOfCustody = newEntry;
            }
            else
            {
                evidence.ChainOfCustody += "\n" + newEntry;
            }

            await _context.SaveChangesAsync();
            return evidence;
        }
    }
}