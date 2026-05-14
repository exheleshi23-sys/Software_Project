using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrafficOfficerApi.Models;
using TrafficOfficerApi.Services;

namespace TrafficOfficerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = "Citizen")] // Commented for testing
public class CitizenController : ControllerBase
{
    private readonly ICitizenService _citizenService;

    public CitizenController(ICitizenService citizenService)
    {
        _citizenService = citizenService;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "GuestCitizen";

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports() => Ok(await _citizenService.GetMyReportsAsync(GetUserId()));

    [HttpPost("reports")]
    public async Task<IActionResult> PostReport([FromBody] CitizenReport report)
    {
        report.CitizenId = GetUserId();
        return Ok(await _citizenService.CreateReportAsync(report));
    }

    [HttpGet("fines")]
    public async Task<IActionResult> GetFines() => Ok(await _citizenService.GetMyFinesAsync(GetUserId()));

    [HttpPost("fines/{id}/pay")]
    public async Task<IActionResult> Pay(int id)
    {
        var success = await _citizenService.PayFineAsync(id);
        return success ? Ok(new { message = "Payment successful" }) : NotFound();
    }

    [HttpGet("profile")]
    public IActionResult GetProfile() => Ok(new { Name = "John Doe", Role = "Citizen", ID = GetUserId() });
}