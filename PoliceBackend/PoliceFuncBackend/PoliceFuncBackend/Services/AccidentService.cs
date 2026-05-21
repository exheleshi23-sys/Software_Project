using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;
using System.Data;

namespace PoliceFuncBackend.Services
{
    public class AccidentService : IAccidentService
    {
        private readonly PoliceDbContext _db;

        public AccidentService(PoliceDbContext db)
        {
            _db = db;
        }

        // -----------------------------------
        // GET ALL ACCIDENTS
        // -----------------------------------
        public async Task<IEnumerable<AccidentDto>> GetAccidentsAsync()
        {
            var accidents = new List<AccidentDto>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT 
                    Accident_ID,
                    AccidentDate,
                    AccidentTime,
                    Location,
                    Description,
                    Severity,
                    Status,
                    USER_ID
                FROM traffic_accident";

            using var cmd = new MySqlCommand(query, conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                accidents.Add(new AccidentDto
                {
                    Accident_ID = reader.GetInt32("Accident_ID"),
                    AccidentDate = reader.GetDateTime("AccidentDate"),
                    AccidentTime = reader.GetString("AccidentTime"),
                    Location = reader.GetString("Location"),
                    Description = reader.GetString("Description"),
                    Severity = reader.GetInt32("Severity"),
                    Status = reader.GetString("Status"),
                    USER_ID = reader.IsDBNull(reader.GetOrdinal("USER_ID"))
                        ? null
                        : reader.GetString("USER_ID")
                });
            }

            return accidents;
        }

        // -----------------------------------
        // GET ACCIDENT BY ID
        // -----------------------------------
        public async Task<AccidentDto?> GetAccidentByIdAsync(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT 
                    Accident_ID,
                    AccidentDate,
                    AccidentTime,
                    Location,
                    Description,
                    Severity,
                    Status,
                    USER_ID
                FROM traffic_accident
                WHERE Accident_ID = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new AccidentDto
            {
                Accident_ID = reader.GetInt32("Accident_ID"),
                AccidentDate = reader.GetDateTime("AccidentDate"),
                AccidentTime = reader.GetString("AccidentTime"),
                Location = reader.GetString("Location"),
                Description = reader.GetString("Description"),
                Severity = reader.GetInt32("Severity"),
                Status = reader.GetString("Status"),
                USER_ID = reader.IsDBNull(reader.GetOrdinal("USER_ID"))
                    ? null
                    : reader.GetString("USER_ID")
            };
        }

        // -----------------------------------
        // CREATE ACCIDENT
        // -----------------------------------
        public async Task<AccidentDto> CreateAccidentAsync(
            CreateAccidentDto dto,
            string officerId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            int randomId = new Random().Next(1000, 999999);

            string query = @"
                INSERT INTO traffic_accident
                (
                    Accident_ID,
                    Location,
                    Description,
                    AccidentDate,
                    AccidentTime,
                    Severity,
                    Status,
                    USER_ID
                )
                VALUES
                (
                    @Accident_ID,
                    @Location,
                    @Description,
                    @AccidentDate,
                    @AccidentTime,
                    @Severity,
                    @Status,
                    @USER_ID
                )";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@Accident_ID", randomId);
            cmd.Parameters.AddWithValue("@Location", dto.Location);
            cmd.Parameters.AddWithValue("@Description", dto.Description);
            cmd.Parameters.AddWithValue("@AccidentDate", DateTime.UtcNow.Date);
            cmd.Parameters.AddWithValue("@AccidentTime", DateTime.Now.ToString("HH:mm"));
            cmd.Parameters.AddWithValue("@Severity", dto.Severity);
            cmd.Parameters.AddWithValue("@Status", "Open");
            cmd.Parameters.AddWithValue("@USER_ID", officerId);

            await cmd.ExecuteNonQueryAsync();

            return new AccidentDto
            {
                Accident_ID = randomId,
                Location = dto.Location,
                Description = dto.Description,
                AccidentDate = DateTime.UtcNow.Date,
                AccidentTime = DateTime.Now.ToString("HH:mm"),
                Severity = dto.Severity,
                Status = "Open",
                USER_ID = officerId
            };
        }

        // -----------------------------------
        // UPDATE ACCIDENT
        // -----------------------------------
        public async Task<bool> UpdateAccidentAsync(
            int id,
            UpdateAccidentDto dto)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                UPDATE traffic_accident
                SET
                    Description = @Description,
                    Severity = @Severity,
                    Status = @Status
                WHERE Accident_ID = @id";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@Description", dto.Description);
            cmd.Parameters.AddWithValue("@Severity", dto.Severity);
            cmd.Parameters.AddWithValue("@Status", dto.Status);
            cmd.Parameters.AddWithValue("@id", id);

            int rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }
    }
}