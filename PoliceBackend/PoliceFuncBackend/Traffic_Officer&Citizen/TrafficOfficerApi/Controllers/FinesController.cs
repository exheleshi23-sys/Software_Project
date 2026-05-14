using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrafficOfficerApi.DTOs;
using TrafficOfficerApi.Services;

namespace TrafficOfficerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FinesController : ControllerBase
{
    private readonly IFineService _fineService;

    public FinesController(IFineService fineService)
    {
        _fineService = fineService;
    }

    [HttpGet]
    [Authorize(Roles = "Traffic, Admin")]
    public async Task<IActionResult> GetFines([FromQuery] DateTime? date, [FromQuery] string? status, [FromQuery] string? plate)
    {
        var fines = await _fineService.GetFinesAsync(date, status, plate);
        return Ok(fines);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Traffic, Admin")]
    public async Task<IActionResult> GetFine(int id)
    {
        var fine = await _fineService.GetFineByIdAsync(id);
        if (fine == null) return NotFound();
        return Ok(fine);
    }

    [HttpPost]
    [Authorize(Roles = "Traffic")]
    public async Task<IActionResult> CreateFine([FromForm] CreateFineDto dto)
    {
        var officerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Unknown"; 
        var fine = await _fineService.CreateFineAsync(dto, officerId);
        return CreatedAtAction(nameof(GetFine), new { id = fine.Id }, fine);
    }

    [HttpPut("{id}/cancel")]
    [Authorize(Roles = "Traffic, Admin")]
    public async Task<IActionResult> CancelFine(int id)
    {
        var result = await _fineService.CancelFineAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("stats/by-type")]
    [Authorize(Roles = "Traffic")]
    public async Task<IActionResult> GetStatsByType()
    {
        return Ok(await _fineService.GetFinesStatsByTypeAsync());
    }

    [HttpGet("stats/daily")]
    [Authorize(Roles = "Traffic")]
    public async Task<IActionResult> GetDailyStats()
    {
        return Ok(await _fineService.GetDailyFinesStatsAsync());
    }
}
