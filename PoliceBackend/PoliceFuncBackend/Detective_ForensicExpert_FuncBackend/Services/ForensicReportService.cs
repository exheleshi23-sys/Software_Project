using Microsoft.EntityFrameworkCore;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public class ForensicReportService : IForensicReportService
    {
        private readonly ApplicationDbContext _context;

        public ForensicReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ForensicReport>> GetAllReportsAsync()
        {
            return await _context.ForensicReports.ToListAsync();
        }

        public async Task<ForensicReport?> GetReportByIdAsync(int id)
        {
            return await _context.ForensicReports.FindAsync(id);
        }

        public async Task<ForensicReport> CreateReportAsync(CreateForensicReportDto dto)
        {
            var report = new ForensicReport
            {
                InvestigationId = dto.InvestigationId,
                InvestigationStatus = dto.InvestigationStatus,
                Summary = dto.Summary,
                EvidenceAnalysis = dto.EvidenceAnalysis,
                SuspectAssessment = dto.SuspectAssessment,
                InvestigativeConclusions = dto.InvestigativeConclusions,
                Evidence = dto.Evidence,
                ReportId = dto.ReportId,
                DetectiveId = dto.DetectiveId
            };

            _context.ForensicReports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<ForensicReport?> UpdateReportAsync(int id, UpdateForensicReportDto dto)
        {
            var report = await _context.ForensicReports.FindAsync(id);

            if (report == null)
            {
                return null;
            }

            report.InvestigationStatus = dto.InvestigationStatus;
            report.Summary = dto.Summary;
            report.EvidenceAnalysis = dto.EvidenceAnalysis;
            report.SuspectAssessment = dto.SuspectAssessment;
            report.InvestigativeConclusions = dto.InvestigativeConclusions;
            report.Evidence = dto.Evidence;

            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<ForensicReport?> SubmitReportAsync(int id)
        {
            var report = await _context.ForensicReports.FindAsync(id);

            if (report == null)
            {
                return null;
            }

            report.InvestigationStatus = "submitted";

            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<List<ForensicReport>> GetReportsByCaseIdAsync(int caseId)
        {
            return await _context.ForensicReports
                .Where(r => r.ReportId == caseId)
                .ToListAsync();
        }
    }
}