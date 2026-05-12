using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.Services;
using PoliceFuncBackend.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace PoliceFuncBackend.Controllers
{
    [ApiController]
    [Route("api/arrests")]
    public class ArrestsController : ControllerBase
    {
        private readonly ArrestService _service;

        public ArrestsController(ArrestService service)
        {
            _service = service;
        }

        // GET /api/arrests
        [HttpGet]
        [Authorize(Roles = "Officer,Admin")]
        public IActionResult GetAll()
        {
            return Ok(_service.GetAll());
        }

        // GET /api/arrests/:id
        [HttpGet("{id}")]
        [Authorize(Roles = "Officer,Admin")]
        public IActionResult GetById(int id)
        {
            var result = _service.GetById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // POST /api/arrests
        [HttpPost]
        [Authorize(Roles = "Officer,Admin")]
        public IActionResult Create([FromBody] ArrestCreateDto dto)
        {
            _service.Create(dto);
            return Ok("Arrest created successfully");
        }

        // PUT /api/arrests/:id/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Officer,Admin")]
        public IActionResult UpdateStatus(int id, [FromBody] ArrestStatusDto dto)
        {
            _service.UpdateStatus(id, dto.Status);
            return Ok("Status updated");
        }
    }
}