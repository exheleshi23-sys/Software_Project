using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Services;

namespace PoliceFuncBackend.Controllers
{
    [ApiController]
    [Route("api/forensic-reports")]
    public class ForensicReportsController : ControllerBase
    {
        private readonly IForensicReportService _reportService;

        public ForensicReportsController(
            IForensicReportService reportService)
        {
            _reportService = reportService;
        }

        // -----------------------------------
        // GET ALL REPORTS
        // GET REPORTS BY CASE ID
        // -----------------------------------
        [HttpGet]
        public async Task<IActionResult> GetReports(
            [FromQuery(Name = "case_id")] int? caseId)
        {
            if (caseId.HasValue)
            {
                var reportsByCase =
                    await _reportService
                        .GetReportsByCaseIdAsync(caseId.Value);

                return Ok(reportsByCase);
            }

            var reports =
                await _reportService.GetAllReportsAsync();

            return Ok(reports);
        }

        // -----------------------------------
        // CREATE REPORT
        // -----------------------------------
        [HttpPost]
        public async Task<IActionResult> CreateReport(
            [FromBody] CreateForensicReportDto dto)
        {
            var report =
                await _reportService.CreateReportAsync(dto);

            return Ok(report);
        }

        // -----------------------------------
        // GET REPORT BY ID
        // -----------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportById(int id)
        {
            var report =
                await _reportService.GetReportByIdAsync(id);

            if (report == null)
                return NotFound();

            return Ok(report);
        }

        // -----------------------------------
        // UPDATE REPORT
        // -----------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReport(
            int id,
            [FromBody] UpdateForensicReportDto dto)
        {
            var report =
                await _reportService.UpdateReportAsync(id, dto);

            if (report == null)
                return NotFound();

            return Ok(report);
        }

        // -----------------------------------
        // SUBMIT REPORT
        // -----------------------------------
        [HttpPut("{id}/submit")]
        public async Task<IActionResult> SubmitReport(int id)
        {
            var report =
                await _reportService.SubmitReportAsync(id);

            if (report == null)
                return NotFound();

            return Ok(report);
        }
    }
}