using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrafficOfficerApi.DTOs;
using TrafficOfficerApi.Services;
using System.Security.Claims;

namespace TrafficOfficerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "Traffic")]
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
        return Ok(await _accidentService.GetAccidentsAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccident(int id)
    {
        var accident = await _accidentService.GetAccidentByIdAsync(id);
        if (accident == null) return NotFound();
        return Ok(accident);
    }

    [HttpPost]
    public async Task<IActionResult> ReportAccident([FromBody] CreateAccidentDto dto)
    {
        var officerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        var result = await _accidentService.CreateAccidentAsync(dto, officerId);
        return CreatedAtAction(nameof(GetAccident), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccident(int id, [FromBody] UpdateAccidentDto dto)
    {
        var success = await _accidentService.UpdateAccidentAsync(id, dto);
        if (!success) return NotFound();
        return NoContent();
    }
}