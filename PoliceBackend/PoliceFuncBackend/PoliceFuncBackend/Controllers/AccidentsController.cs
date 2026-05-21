using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Services;

namespace PoliceFuncBackend.Controllers
{
    [Route("api/accidents")]
    [ApiController]
    [Authorize(Roles = "Traffic")]
    public class AccidentsController : ControllerBase
    {
        private readonly IAccidentService _accidentService;

        public AccidentsController(IAccidentService accidentService)
        {
            _accidentService = accidentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccidents()
        {
            var accidents = await _accidentService.GetAccidentsAsync();

            return Ok(accidents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccident(int id)
        {
            var accident = await _accidentService.GetAccidentByIdAsync(id);

            if (accident == null)
                return NotFound("Accident not found");

            return Ok(accident);
        }

        [HttpPost]
        public async Task<IActionResult> ReportAccident([FromBody] CreateAccidentDto dto)
        {
            var officerId = User.FindFirst("userId")?.Value;

            if (officerId == null)
                return Unauthorized("Invalid token");

            var result = await _accidentService.CreateAccidentAsync(dto, officerId);

            return CreatedAtAction(
                nameof(GetAccident),
                new { id = result.Accident_ID },
                result
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccident(
            int id,
            [FromBody] UpdateAccidentDto dto)
        {
            var success =
                await _accidentService.UpdateAccidentAsync(id, dto);

            if (!success)
                return NotFound("Accident not found");

            return NoContent();
        }
    }
}