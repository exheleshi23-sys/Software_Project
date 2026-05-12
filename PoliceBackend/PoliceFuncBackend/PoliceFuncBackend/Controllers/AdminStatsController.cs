using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.Services;

namespace PoliceFuncBackend.Controllers
{
    [ApiController]
    [Route("api/admin/stats")]
    public class AdminStatsController : ControllerBase
    {
        private readonly AdminStatsService _stats;

        public AdminStatsController(AdminStatsService stats)
        {
            _stats = stats;
        }

        private bool IsAdmin() => true;

        [HttpGet("overview")]
        public IActionResult Overview()
        {
            if (!IsAdmin()) return Unauthorized();
            return Ok(_stats.GetOverview());
        }

        [HttpGet("cases-by-type")]
        public IActionResult CasesByType()
        {
            if (!IsAdmin()) return Unauthorized();
            return Ok(_stats.GetCasesByType());
        }

        [HttpGet("monthly-activity")]
        public IActionResult MonthlyActivity()
        {
            if (!IsAdmin()) return Unauthorized();
            return Ok(_stats.GetMonthlyActivity());
        }
    }
}