using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Models;
using System.Data;

namespace PoliceFuncBackend.Services
{
    public class ForensicReportService : IForensicReportService
    {
        private readonly PoliceDbContext _db;

        public ForensicReportService(PoliceDbContext db)
        {
            _db = db;
        }

        // -----------------------------------
        // GET ALL REPORTS
        // -----------------------------------
        public async Task<List<ForensicReport>> GetAllReportsAsync()
        {
            var reports = new List<ForensicReport>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = "SELECT * FROM investigation_report";

            using var cmd = new MySqlCommand(query, conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reports.Add(new ForensicReport
                {
                    Investigation_ID = reader.GetInt32("Investigation_ID"),
                    Investigation_Status = reader.GetString("Investigation_Status"),
                    Summary = reader.GetString("Summary"),
                    Evidence_analysis = reader.GetString("Evidence_analysis"),
                    Suspect_assessment = reader.GetString("Suspect_assessment"),
                    Investigative_Conclusions = reader.GetString("Investigative_Conclusions"),
                    Evidence = reader.GetString("Evidence"),
                    Report_ID = reader.GetInt32("Report_ID"),
                    Detective_ID = reader.IsDBNull(reader.GetOrdinal("Detective_ID"))
                        ? null
                        : reader.GetString("Detective_ID")
                });
            }

            return reports;
        }

        // -----------------------------------
        // GET REPORT BY ID
        // -----------------------------------
        public async Task<ForensicReport?> GetReportByIdAsync(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT *
                FROM investigation_report
                WHERE Investigation_ID = @id";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new ForensicReport
            {
                Investigation_ID = reader.GetInt32("Investigation_ID"),
                Investigation_Status = reader.GetString("Investigation_Status"),
                Summary = reader.GetString("Summary"),
                Evidence_analysis = reader.GetString("Evidence_analysis"),
                Suspect_assessment = reader.GetString("Suspect_assessment"),
                Investigative_Conclusions = reader.GetString("Investigative_Conclusions"),
                Evidence = reader.GetString("Evidence"),
                Report_ID = reader.GetInt32("Report_ID"),
                Detective_ID = reader.IsDBNull(reader.GetOrdinal("Detective_ID"))
                    ? null
                    : reader.GetString("Detective_ID")
            };
        }

        // -----------------------------------
        // CREATE REPORT
        // -----------------------------------
        public async Task<ForensicReport> CreateReportAsync(
            CreateForensicReportDto dto)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                INSERT INTO investigation_report
                (
                    Investigation_ID,
                    Investigation_Status,
                    Summary,
                    Evidence_analysis,
                    Suspect_assessment,
                    Investigative_Conclusions,
                    Evidence,
                    Report_ID,
                    Detective_ID
                )
                VALUES
                (
                    @Investigation_ID,
                    @Investigation_Status,
                    @Summary,
                    @Evidence_analysis,
                    @Suspect_assessment,
                    @Investigative_Conclusions,
                    @Evidence,
                    @Report_ID,
                    @Detective_ID
                )";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue(
                "@Investigation_ID",
                dto.Investigation_ID
            );

            cmd.Parameters.AddWithValue(
                "@Investigation_Status",
                dto.Investigation_Status
            );

            cmd.Parameters.AddWithValue(
                "@Summary",
                dto.Summary
            );

            cmd.Parameters.AddWithValue(
                "@Evidence_analysis",
                dto.Evidence_Analysis
            );

            cmd.Parameters.AddWithValue(
                "@Suspect_assessment",
                dto.Suspect_Assessment
            );

            cmd.Parameters.AddWithValue(
                "@Investigative_Conclusions",
                dto.Investigative_Conclusions
            );

            cmd.Parameters.AddWithValue(
                "@Evidence",
                dto.Evidence
            );

            cmd.Parameters.AddWithValue(
                "@Report_ID",
                dto.Report_ID
            );

            cmd.Parameters.AddWithValue(
                "@Detective_ID",
                dto.Detective_ID
            );

            await cmd.ExecuteNonQueryAsync();

            return new ForensicReport
            {
                Investigation_ID = dto.Investigation_ID,
                Investigation_Status = dto.Investigation_Status,
                Summary = dto.Summary,
                Evidence_analysis = dto.Evidence_Analysis,
                Suspect_assessment = dto.Suspect_Assessment,
                Investigative_Conclusions = dto.Investigative_Conclusions,
                Evidence = dto.Evidence,
                Report_ID = dto.Report_ID,
                Detective_ID = dto.Detective_ID
            };
        }

        // -----------------------------------
        // UPDATE REPORT
        // -----------------------------------
        public async Task<ForensicReport?> UpdateReportAsync(
            int id,
            UpdateForensicReportDto dto)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                UPDATE investigation_report
                SET
                    Investigation_Status = @Investigation_Status,
                    Summary = @Summary,
                    Evidence_analysis = @Evidence_analysis,
                    Suspect_assessment = @Suspect_assessment,
                    Investigative_Conclusions = @Investigative_Conclusions,
                    Evidence = @Evidence
                WHERE Investigation_ID = @id";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);

            cmd.Parameters.AddWithValue(
                "@Investigation_Status",
                dto.Investigation_Status
            );

            cmd.Parameters.AddWithValue(
                "@Summary",
                dto.Summary
            );

            cmd.Parameters.AddWithValue(
                "@Evidence_analysis",
                dto.Evidence_Analysis
            );

            cmd.Parameters.AddWithValue(
                "@Suspect_assessment",
                dto.Suspect_Assessment
            );

            cmd.Parameters.AddWithValue(
                "@Investigative_Conclusions",
                dto.Investigative_Conclusions
            );

            cmd.Parameters.AddWithValue(
                "@Evidence",
                dto.Evidence
            );

            int rows = await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return null;

            return await GetReportByIdAsync(id);
        }

        // -----------------------------------
        // SUBMIT REPORT
        // -----------------------------------
        public async Task<ForensicReport?> SubmitReportAsync(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                UPDATE investigation_report
                SET Investigation_Status = 'submitted'
                WHERE Investigation_ID = @id";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);

            int rows = await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return null;

            return await GetReportByIdAsync(id);
        }

        // -----------------------------------
        // GET REPORTS BY CASE ID
        // -----------------------------------
        public async Task<List<ForensicReport>> GetReportsByCaseIdAsync(
            int caseId)
        {
            var reports = new List<ForensicReport>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT ir.*
                FROM investigation_report ir
                INNER JOIN incident_report rp
                    ON ir.Report_ID = rp.Report_ID
                WHERE rp.Case_ID = @Case_ID";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@Case_ID", caseId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reports.Add(new ForensicReport
                {
                    Investigation_ID = reader.GetInt32("Investigation_ID"),
                    Investigation_Status = reader.GetString("Investigation_Status"),
                    Summary = reader.GetString("Summary"),
                    Evidence_analysis = reader.GetString("Evidence_analysis"),
                    Suspect_assessment = reader.GetString("Suspect_assessment"),
                    Investigative_Conclusions = reader.GetString("Investigative_Conclusions"),
                    Evidence = reader.GetString("Evidence"),
                    Report_ID = reader.GetInt32("Report_ID"),
                    Detective_ID = reader.IsDBNull(reader.GetOrdinal("Detective_ID"))
                        ? null
                        : reader.GetString("Detective_ID")
                });
            }

            return reports;
        }
    }
}