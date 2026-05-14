using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Services;

namespace PoliceFuncBackend.Controllers
{
    [Route("api/forensic-reports")]
    [ApiController]
    public class ForensicReportsController : ControllerBase
    {
        private readonly IForensicReportService _reportService;

        public ForensicReportsController(IForensicReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<IActionResult> GetReports([FromQuery(Name = "case_id")] int? caseId)
        {
            if (caseId.HasValue)
            {
                var reportsByCase = await _reportService.GetReportsByCaseIdAsync(caseId.Value);
                return Ok(reportsByCase);
            }

            var reports = await _reportService.GetAllReportsAsync();
            return Ok(reports);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport(CreateForensicReportDto dto)
        {
            var report = await _reportService.CreateReportAsync(dto);
            return Ok(report);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportById(int id)
        {
            var report = await _reportService.GetReportByIdAsync(id);

            if (report == null)
            {
                return NotFound();
            }

            return Ok(report);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReport(int id, UpdateForensicReportDto dto)
        {
            var report = await _reportService.UpdateReportAsync(id, dto);

            if (report == null)
            {
                return NotFound();
            }

            return Ok(report);
        }

        [HttpPut("{id}/submit")]
        public async Task<IActionResult> SubmitReport(int id)
        {
            var report = await _reportService.SubmitReportAsync(id);

            if (report == null)
            {
                return NotFound();
            }

            return Ok(report);
        }
    }
}