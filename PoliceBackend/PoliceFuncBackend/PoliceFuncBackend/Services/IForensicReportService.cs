using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public interface IForensicReportService
    {
        Task<List<ForensicReport>> GetAllReportsAsync();

        Task<ForensicReport?> GetReportByIdAsync(int id);

        Task<ForensicReport> CreateReportAsync(CreateForensicReportDto dto);

        Task<ForensicReport?> UpdateReportAsync(
            int id,
            UpdateForensicReportDto dto
        );

        Task<ForensicReport?> SubmitReportAsync(int id);

        Task<List<ForensicReport>> GetReportsByCaseIdAsync(int caseId);
    }
}