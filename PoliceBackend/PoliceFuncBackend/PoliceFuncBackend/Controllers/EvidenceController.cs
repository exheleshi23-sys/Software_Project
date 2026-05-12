using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.Services;
using PoliceFuncBackend.DTOs;

namespace PoliceFuncBackend.Controllers
{
    [ApiController]
    [Route("api/evidence")]
    public class EvidenceController : ControllerBase
    {
        private readonly EvidenceService _service;

        public EvidenceController(EvidenceService service)
        {
            _service = service;
        }

        // GET /api/evidence?case_id=1
        [HttpGet]
        public IActionResult GetAll([FromQuery] int? case_id)
        {
            return Ok(_service.GetEvidence(case_id));
        }

        // GET /api/evidence/:id
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _service.GetById(id);
            if (result == null) return NotFound();

            return Ok(result);
        }

        // POST /api/evidence
        [HttpPost]
        public IActionResult Create([FromBody] EvidenceCreateDto dto)
        {
            _service.CreateEvidence(dto);
            return Ok("Evidence created");
        }

        // PUT /api/evidence/:id/transfer
        [HttpPut("{id}/transfer")]
        public IActionResult Transfer(int id, [FromBody] EvidenceTransferDto dto)
        {
            _service.TransferEvidence(id, dto);
            return Ok("Evidence transferred");
        }
    }
}