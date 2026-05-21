using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.Services;

namespace PoliceFuncBackend.Controllers
{
    [Route("api/vehicles")]
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

            if (vehicle == null)
                return NotFound(new { message = "Vehicle not found." });

            return Ok(vehicle);
        }
    }
}