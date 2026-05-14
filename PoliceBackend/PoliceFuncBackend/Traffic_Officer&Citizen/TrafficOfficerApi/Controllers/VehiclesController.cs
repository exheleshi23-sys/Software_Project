using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrafficOfficerApi.Services;

namespace TrafficOfficerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Traffic")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet("lookup/{plate}")]
    public async Task<IActionResult> LookupVehicle(string plate)
    {
        var vehicle = await _vehicleService.LookupVehicleAsync(plate);
        if (vehicle == null) return NotFound(new { message = "Vehicle not found in external registry." });
        return Ok(vehicle);
    }
}