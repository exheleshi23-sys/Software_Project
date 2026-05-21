using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Services;

namespace PoliceFuncBackend.Controllers
{
    [Route("api/fines")]
    [ApiController]
    public class FinesController : ControllerBase
    {
        private readonly IFineService _fineService;

        public FinesController(IFineService fineService)
        {
            _fineService = fineService;
        }

        [HttpGet]
        [Authorize(Roles = "Traffic,Admin")]
        public async Task<IActionResult> GetFines(
            [FromQuery] DateTime? date,
            [FromQuery] string? status,
            [FromQuery] string? plate)
        {
            var fines = await _fineService.GetFinesAsync(date, status, plate);
            return Ok(fines);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Traffic,Admin")]
        public async Task<IActionResult> GetFine(int id)
        {
            var fine = await _fineService.GetFineByIdAsync(id);

            if (fine == null)
                return NotFound("Fine not found");

            return Ok(fine);
        }

        [HttpPost]
        [Authorize(Roles = "Traffic")]
        public async Task<IActionResult> CreateFine([FromForm] CreateFineDto dto)
        {
            var officerId = User.FindFirst("userId")?.Value;

            if (officerId == null)
                return Unauthorized("Invalid token");

            var fine = await _fineService.CreateFineAsync(dto, officerId);

            return CreatedAtAction(
                nameof(GetFine),
                new { id = fine.Fine_ID },
                fine
            );
        }

        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Traffic,Admin")]
        public async Task<IActionResult> CancelFine(int id)
        {
            var result = await _fineService.CancelFineAsync(id);

            if (!result)
                return NotFound("Fine not found");

            return NoContent();
        }

        [HttpGet("stats/by-type")]
        [Authorize(Roles = "Traffic")]
        public async Task<IActionResult> GetStatsByType()
        {
            var stats = await _fineService.GetFinesStatsByTypeAsync();
            return Ok(stats);
        }

        [HttpGet("stats/daily")]
        [Authorize(Roles = "Traffic")]
        public async Task<IActionResult> GetDailyStats()
        {
            var stats = await _fineService.GetDailyFinesStatsAsync();
            return Ok(stats);
        }
    }
}