using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Services;

namespace PoliceFuncBackend.Controllers
{
    [Route("api/cases")]
    [ApiController]
    public class CasesController : ControllerBase
    {
        private readonly ICaseService _caseService;

        public CasesController(ICaseService caseService)
        {
            _caseService = caseService;
        }

        [HttpGet("assigned-to-me")]
        public async Task<IActionResult> GetAssignedCases()
        {
            var cases = await _caseService.GetAssignedCasesAsync();
            return Ok(cases);
        }

        [HttpGet("{id}/notes")]
        public async Task<IActionResult> GetCaseNotes(int id)
        {
            var notes = await _caseService.GetCaseNotesAsync(id);
            return Ok(notes);
        }

        [HttpPost("{id}/notes")]
        public async Task<IActionResult> AddCaseNote(int id, CreateCaseNoteDto dto)
        {
            var note = await _caseService.AddCaseNoteAsync(id, dto);
            return Ok(note);
        }
    }
}