using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.Services;
using PoliceFuncBackend.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace PoliceFuncBackend.Controllers
{
    [ApiController]
    [Route("api/cases")]
    public class CasesController : ControllerBase
    {
        private readonly ICaseService _caseService;

        public CasesController(ICaseService caseService)
        {
            _caseService = caseService;
        }

        private bool IsOfficerPlus() => true;
        private bool IsOfficer() => true;

        // GET /cases
        [HttpGet]
        public IActionResult GetCases(string status, string priority)
        {
            if (!IsOfficerPlus()) return Unauthorized();
            return Ok(_caseService.GetCases(status, priority));
        }

        // GET /cases/my
        [Authorize]
        [HttpGet("my")]
        public IActionResult GetMyCases()
        {
            // check role from JWT
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Officer")
                return Forbid();

            // get userId from JWT
            var userId = User.FindFirst("userId")?.Value;

            if (userId == null)
                return Unauthorized();

            return Ok(_caseService.GetMyCases(userId));
        }

        // GET /cases/:id
        [HttpGet("{id}")]
        public IActionResult GetCase(int id)
        {
            if (!IsOfficerPlus()) return Unauthorized();

            var c = _caseService.GetCaseById(id);
            if (c == null) return NotFound();

            return Ok(c);
        }

        // POST /cases
        [HttpPost]
        public IActionResult Create(CaseDto dto)
        {
            if (!IsOfficerPlus()) return Unauthorized();
            _caseService.CreateCase(dto);
            return Ok();
        }

        // PUT /cases/:id
        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateCaseDto dto)
        {
            if (!IsOfficerPlus()) return Unauthorized();
            _caseService.UpdateCase(id, dto);
            return Ok();
        }

        // PUT /status
        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id, string status)
        {
            if (!IsOfficerPlus()) return Unauthorized();
            _caseService.UpdateStatus(id, status);
            return Ok();
        }

        // ASSIGN
        [HttpPut("{id}/assign")]
        public IActionResult Assign(int id, AssignCaseDto dto)
        {
            if (!IsOfficerPlus()) return Unauthorized();
            _caseService.AssignCase(id, dto.User_ID);
            return Ok();
        }


        // -----------------------------------
        // GET CASE NOTES
        // -----------------------------------
        [HttpGet("{id}/notes")]
        public async Task<IActionResult> GetCaseNotes(int id)
        {
            var notes =
                await _caseService.GetCaseNotesAsync(id);

            return Ok(notes);
        }

        // -----------------------------------
        // ADD CASE NOTE
        // -----------------------------------
        [HttpPost("{id}/notes")]
        public async Task<IActionResult> AddCaseNote(
            int id,
            [FromBody] CreateCaseNoteDto dto)
        {
            var note =
                await _caseService.AddCaseNoteAsync(id, dto);

            return Ok(note);
        }

        // -----------------------------------
        // GET FORENSIC REPORTS
        // -----------------------------------
        [HttpGet("{id}/forensic-reports")]
        public async Task<IActionResult> GetForensicReports(int id)
        {
            var reports =
                await _caseService.GetForensicReportsAsync(id);

            return Ok(reports);
        }
    }
}