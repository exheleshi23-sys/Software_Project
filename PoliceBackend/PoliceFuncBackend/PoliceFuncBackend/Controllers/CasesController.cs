using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.Services;
using PoliceFuncBackend.DTOs;

namespace PoliceFuncBackend.Controllers
{
    [ApiController]
    [Route("api/cases")]
    public class CasesController : ControllerBase
    {
        private readonly CaseService _caseService;

        public CasesController(CaseService caseService)
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
        [HttpGet("my")]
        public IActionResult GetMyCases()
        {
            if (!IsOfficer()) return Unauthorized();
            return Ok(_caseService.GetMyCases(1)); // replace with JWT later
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
    }
}