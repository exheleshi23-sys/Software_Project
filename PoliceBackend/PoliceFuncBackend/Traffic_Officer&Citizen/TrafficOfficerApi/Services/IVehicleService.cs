using TrafficOfficerApi.Models;

namespace TrafficOfficerApi.Services;

public interface IVehicleService
{
    Task<Vehicle?> LookupVehicleAsync(string plate);
}
