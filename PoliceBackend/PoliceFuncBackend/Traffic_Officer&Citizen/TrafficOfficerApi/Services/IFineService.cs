using TrafficOfficerApi.DTOs;

namespace TrafficOfficerApi.Services;

public interface IFineService
{
    Task<IEnumerable<FineDto>> GetFinesAsync(DateTime? date, string? status, string? plate);
    Task<FineDto?> GetFineByIdAsync(int id);
    Task<FineDto> CreateFineAsync(CreateFineDto createFineDto, string officerId);
    Task<bool> CancelFineAsync(int id);
    Task<object> GetFinesStatsByTypeAsync();
    Task<object> GetDailyFinesStatsAsync();
}
