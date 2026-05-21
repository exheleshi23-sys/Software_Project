using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public interface ICaseService
    {
        List<object> GetCases(string? status = null, string? priority = null);

        object? GetCaseById(int id);

        void CreateCase(CaseDto dto);

        void UpdateCase(int id, UpdateCaseDto dto);

        void UpdateStatus(int id, string status);

        void AssignCase(int caseId, string userId);

        List<object> GetMyCases(string officerId);

        Task<List<Case>> GetAssignedCasesAsync();

        Task<List<CaseNote>> GetCaseNotesAsync(int caseId);

        Task<CaseNote> AddCaseNoteAsync(
            int caseId,
            CreateCaseNoteDto dto
        );

        Task<List<ForensicReport>> GetForensicReportsAsync(
            int caseId
        );
    }
}