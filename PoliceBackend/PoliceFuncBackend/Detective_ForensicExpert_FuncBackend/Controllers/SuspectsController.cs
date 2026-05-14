using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Services;

namespace PoliceFuncBackend.Controllers
{
    [Route("api/suspects")]
    [ApiController]
    public class SuspectsController : ControllerBase
    {
        private readonly ISuspectService _suspectService;

        public SuspectsController(ISuspectService suspectService)
        {
            _suspectService = suspectService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSuspects()
        {
            var suspects = await _suspectService.GetAllSuspectsAsync();
            return Ok(suspects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSuspectById(int id)
        {
            var suspect = await _suspectService.GetSuspectByIdAsync(id);

            if (suspect == null)
            {
                return NotFound();
            }

            return Ok(suspect);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSuspect(CreateSuspectDto dto)
        {
            var suspect = await _suspectService.CreateSuspectAsync(dto);
            return CreatedAtAction(nameof(GetSuspectById), new { id = suspect.SuspectId }, suspect);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSuspect(int id, UpdateSuspectDto dto)
        {
            var suspect = await _suspectService.UpdateSuspectAsync(id, dto);

            if (suspect == null)
            {
                return NotFound();
            }

            return Ok(suspect);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSuspect(int id)
        {
            var deleted = await _suspectService.DeleteSuspectAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}