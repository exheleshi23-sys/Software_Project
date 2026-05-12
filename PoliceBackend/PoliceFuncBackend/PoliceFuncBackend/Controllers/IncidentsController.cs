using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.Services;
using PoliceFuncBackend.DTOs;

namespace PoliceFuncBackend.Controllers
{
    [ApiController]
    [Route("api/incidents")]
    public class IncidentsController : ControllerBase
    {
        private readonly IncidentService _service;

        public IncidentsController(IncidentService service)
        {
            _service = service;
        }

        // Officer+
        [HttpGet]
        public IActionResult Get(string type)
        {
            return Ok(_service.GetIncidents(type));
        }

        // Officer
        [HttpPost]
        public IActionResult Create(IncidentDto dto)
        {
            _service.CreateIncident(dto);
            return Ok();
        }

        // Officer+
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var result = _service.GetIncidentById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // Officer
        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateIncidentDto dto)
        {
            _service.UpdateIncident(id, dto);
            return Ok();
        }

        // Officer+
        [HttpPut("{id}/link-case")]
        public IActionResult Link(int id, LinkCaseDto dto)
        {
            _service.LinkCase(id, dto.Case_ID);
            return Ok();
        }
    }
}