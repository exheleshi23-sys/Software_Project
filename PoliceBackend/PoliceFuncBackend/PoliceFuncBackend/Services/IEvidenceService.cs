using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public interface IEvidenceService
    {
        Task<List<Evidence>> GetEvidenceByCaseIdAsync(int caseId);

        Task<List<Evidence>> GetEvidenceQueueAsync();

        Task<Evidence?> AssignAnalystAsync(
            int id,
            AssignAnalystDto dto
        );

        Task<Evidence?> UpdateStatusAsync(
            int id,
            UpdateEvidenceStatusDto dto
        );

        Task<string?> GetChainOfCustodyAsync(int id);

        Task<Evidence?> AddChainOfCustodyAsync(
            int id,
            ChainOfCustodyDto dto
        );
    }
}