using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public interface ICaseService
    {
        Task<List<Case>> GetAssignedCasesAsync();

        Task<List<CaseNote>> GetCaseNotesAsync(int caseId);

        Task<CaseNote> AddCaseNoteAsync(int caseId, CreateCaseNoteDto dto);

        Task<List<ForensicReport>> GetForensicReportsAsync(int caseId);
    }
}