using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public class CitizenService : ICitizenService
    {
        private readonly PoliceDbContext _db;

        public CitizenService(PoliceDbContext db)
        {
            _db = db;
        }

        // -----------------------------------
        // GET MY REPORTS
        // -----------------------------------
        public async Task<IEnumerable<CitizenReport>> GetMyReportsAsync(string citizenId)
        {
            var reports = new List<CitizenReport>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT Id, CitizenId, Description, ReportedAt, Status
                FROM citizen_report
                WHERE CitizenId = @CitizenId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@CitizenId", citizenId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reports.Add(new CitizenReport
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    CitizenId = reader.GetString(reader.GetOrdinal("CitizenId")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    ReportedAt = reader.GetDateTime(reader.GetOrdinal("ReportedAt")),
                    Status = reader.GetString(reader.GetOrdinal("Status"))
                });
            }

            return reports;
        }

        // -----------------------------------
        // CREATE REPORT
        // -----------------------------------
        public async Task<CitizenReport> CreateReportAsync(CitizenReport report)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                INSERT INTO citizen_report
                (CitizenId, Description, ReportedAt, Status)
                VALUES
                (@CitizenId, @Description, @ReportedAt, @Status);
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(query, conn);

            var reportedAt = DateTime.UtcNow;
            var status = "Pending";

            cmd.Parameters.AddWithValue("@CitizenId", report.CitizenId);
            cmd.Parameters.AddWithValue("@Description", report.Description);
            cmd.Parameters.AddWithValue("@ReportedAt", reportedAt);
            cmd.Parameters.AddWithValue("@Status", status);

            var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            report.Id = newId;
            report.ReportedAt = reportedAt;
            report.Status = status;

            return report;
        }

        // -----------------------------------
        // GET MY FINES
        // -----------------------------------
        public async Task<IEnumerable<Fine>> GetMyFinesAsync(string citizenId)
        {
            var fines = new List<Fine>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT Fine_ID, Amount, IssueDate, DueDate, FineStatus, USER_ID, Violation_ID
                FROM traffic_fine
                WHERE USER_ID = @UserId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", citizenId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                fines.Add(new Fine
                {
                    Fine_ID = reader.GetInt32(reader.GetOrdinal("Fine_ID")),
                    Amount = reader.GetInt32(reader.GetOrdinal("Amount")),
                    IssueDate = reader.GetDateTime(reader.GetOrdinal("IssueDate")),
                    DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
                    FineStatus = reader.GetString(reader.GetOrdinal("FineStatus")),
                    USER_ID = reader.IsDBNull(reader.GetOrdinal("USER_ID"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("USER_ID")),
                    Violation_ID = reader.GetInt32(reader.GetOrdinal("Violation_ID"))
                });
            }

            return fines;
        }

        // -----------------------------------
        // PAY FINE
        // -----------------------------------
        public async Task<bool> PayFineAsync(int fineId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                UPDATE traffic_fine
                SET FineStatus = 'Paid'
                WHERE Fine_ID = @FineId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@FineId", fineId);

            int rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }
    }
}