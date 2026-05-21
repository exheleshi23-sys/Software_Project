using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public interface IVehicleService
    {
        Task<Vehicle?> LookupVehicleAsync(string plate);
    }
}