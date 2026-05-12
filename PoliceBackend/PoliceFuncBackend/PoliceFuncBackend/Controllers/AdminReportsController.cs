using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.Services;

namespace PoliceFuncBackend.Controllers
{
    [ApiController]
    [Route("api/admin/reports")]
    public class AdminReportsController : ControllerBase
    {
        private readonly AdminReportService _report;

        public AdminReportsController(AdminReportService report)
        {
            _report = report;
        }

        private bool IsAdmin() => true;

        [HttpGet("export")]
        public IActionResult Export()
        {
            if (!IsAdmin()) return Unauthorized();

            var data = _report.ExportData();
            return Ok(data);
        }
    }
}