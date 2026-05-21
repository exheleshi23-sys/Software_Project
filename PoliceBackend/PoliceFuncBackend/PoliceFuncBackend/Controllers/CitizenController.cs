using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.Models;
using PoliceFuncBackend.Services;

namespace PoliceFuncBackend.Controllers
{
    [Route("api/citizen")]
    [ApiController]
    [Authorize(Roles = "Citizen")]
    public class CitizenController : ControllerBase
    {
        private readonly ICitizenService _citizenService;

        public CitizenController(ICitizenService citizenService)
        {
            _citizenService = citizenService;
        }

        // -----------------------------------
        // GET USER ID FROM JWT
        // -----------------------------------
        private string GetUserId()
        {
            return User.FindFirst("userId")?.Value ?? "GuestCitizen";
        }

        // -----------------------------------
        // GET MY REPORTS
        // -----------------------------------
        [HttpGet("reports")]
        public async Task<IActionResult> GetReports()
        {
            var reports =
                await _citizenService.GetMyReportsAsync(GetUserId());

            return Ok(reports);
        }

        // -----------------------------------
        // CREATE REPORT
        // -----------------------------------
        [HttpPost("reports")]
        public async Task<IActionResult> PostReport(
            [FromBody] CitizenReport report)
        {
            report.CitizenId = GetUserId();

            var created =
                await _citizenService.CreateReportAsync(report);

            return Ok(created);
        }

        // -----------------------------------
        // GET MY FINES
        // -----------------------------------
        [HttpGet("fines")]
        public async Task<IActionResult> GetFines()
        {
            var fines =
                await _citizenService.GetMyFinesAsync(GetUserId());

            return Ok(fines);
        }

        // -----------------------------------
        // PAY FINE
        // -----------------------------------
        [HttpPost("fines/{id}/pay")]
        public async Task<IActionResult> Pay(int id)
        {
            var success =
                await _citizenService.PayFineAsync(id);

            if (!success)
                return NotFound("Fine not found");

            return Ok(new
            {
                message = "Payment successful"
            });
        }

        // -----------------------------------
        // GET PROFILE
        // -----------------------------------
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            return Ok(new
            {
                ID = GetUserId(),
                Role = "Citizen"
            });
        }
    }
}