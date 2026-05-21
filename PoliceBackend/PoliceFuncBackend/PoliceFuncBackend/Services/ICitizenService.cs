using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public interface ICitizenService
    {
        Task<IEnumerable<CitizenReport>> GetMyReportsAsync(string citizenId);
        Task<CitizenReport> CreateReportAsync(CitizenReport report);
        Task<IEnumerable<Fine>> GetMyFinesAsync(string citizenId);
        Task<bool> PayFineAsync(int fineId);
    }
}