using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;

namespace PoliceFuncBackend.Services
{
    public class FineService : IFineService
    {
        private readonly PoliceDbContext _db;

        public FineService(PoliceDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<FineDto>> GetFinesAsync(DateTime? date, string? status, string? plate)
        {
            var fines = new List<FineDto>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT Fine_ID, Amount, IssueDate, DueDate, FineStatus, USER_ID, Violation_ID
                FROM traffic_fine
                WHERE 1=1";

            if (date.HasValue)
                query += " AND DATE(IssueDate) = @Date";

            if (!string.IsNullOrEmpty(status))
                query += " AND FineStatus = @Status";

            using var cmd = new MySqlCommand(query, conn);

            if (date.HasValue)
                cmd.Parameters.AddWithValue("@Date", date.Value.Date);

            if (!string.IsNullOrEmpty(status))
                cmd.Parameters.AddWithValue("@Status", status);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                fines.Add(new FineDto
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

        public async Task<FineDto?> GetFineByIdAsync(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT Fine_ID, Amount, IssueDate, DueDate, FineStatus, USER_ID, Violation_ID
                FROM traffic_fine
                WHERE Fine_ID = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new FineDto
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
            };
        }

        public async Task<FineDto> CreateFineAsync(CreateFineDto dto, string officerId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            int randomId = new Random().Next(1000, 999999);

            string query = @"
                INSERT INTO traffic_fine
                (Fine_ID, Amount, IssueDate, DueDate, FineStatus, USER_ID, Violation_ID)
                VALUES
                (@Fine_ID, @Amount, @IssueDate, @DueDate, @FineStatus, @USER_ID, @Violation_ID)";

            using var cmd = new MySqlCommand(query, conn);

            var issueDate = dto.IssueDate == default ? DateTime.UtcNow.Date : dto.IssueDate;
            var dueDate = dto.DueDate == default ? issueDate.AddDays(30) : dto.DueDate;
            var fineStatus = string.IsNullOrWhiteSpace(dto.FineStatus) ? "Pending" : dto.FineStatus;

            cmd.Parameters.AddWithValue("@Fine_ID", randomId);
            cmd.Parameters.AddWithValue("@Amount", dto.Amount);
            cmd.Parameters.AddWithValue("@IssueDate", issueDate);
            cmd.Parameters.AddWithValue("@DueDate", dueDate);
            cmd.Parameters.AddWithValue("@FineStatus", fineStatus);
            cmd.Parameters.AddWithValue("@USER_ID", officerId);
            cmd.Parameters.AddWithValue("@Violation_ID", dto.Violation_ID);

            await cmd.ExecuteNonQueryAsync();

            return new FineDto
            {
                Fine_ID = randomId,
                Amount = dto.Amount,
                IssueDate = issueDate,
                DueDate = dueDate,
                FineStatus = fineStatus,
                USER_ID = officerId,
                Violation_ID = dto.Violation_ID
            };
        }

        public async Task<bool> CancelFineAsync(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                UPDATE traffic_fine
                SET FineStatus = 'Cancelled'
                WHERE Fine_ID = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            int rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }

        public async Task<object> GetFinesStatsByTypeAsync()
        {
            var stats = new List<object>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT Violation_ID, COUNT(*) AS Count
                FROM traffic_fine
                GROUP BY Violation_ID";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                stats.Add(new
                {
                    Type = reader.GetInt32(reader.GetOrdinal("Violation_ID")),
                    Count = reader.GetInt32(reader.GetOrdinal("Count"))
                });
            }

            return stats;
        }

        public async Task<object> GetDailyFinesStatsAsync()
        {
            var stats = new List<object>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT DATE(IssueDate) AS FineDate, COUNT(*) AS Count
                FROM traffic_fine
                GROUP BY DATE(IssueDate)
                ORDER BY FineDate";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                stats.Add(new
                {
                    Date = reader.GetDateTime(reader.GetOrdinal("FineDate")).ToString("yyyy-MM-dd"),
                    Count = reader.GetInt32(reader.GetOrdinal("Count"))
                });
            }

            return stats;
        }
    }
}