using Microsoft.EntityFrameworkCore;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public class CaseService : ICaseService
    {
        private readonly ApplicationDbContext _context;

        public CaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Case>> GetAssignedCasesAsync()
        {
            return await _context.Cases.ToListAsync();
        }

        public async Task<List<CaseNote>> GetCaseNotesAsync(int caseId)
        {
            return await _context.CaseNotes
                .Where(note => note.CaseId == caseId)
                .ToListAsync();
        }

        public async Task<CaseNote> AddCaseNoteAsync(int caseId, CreateCaseNoteDto dto)
        {
            var note = new CaseNote
            {
                CaseId = caseId,
                NoteText = dto.NoteText,
                CreatedAt = DateTime.Now
            };

            _context.CaseNotes.Add(note);
            await _context.SaveChangesAsync();

            return note;
        }
    }
}