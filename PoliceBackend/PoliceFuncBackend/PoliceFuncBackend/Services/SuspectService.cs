using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Models;
using System.Data;

namespace PoliceFuncBackend.Services
{
    public class SuspectService : ISuspectService
    {
        private readonly PoliceDbContext _db;

        public SuspectService(PoliceDbContext db)
        {
            _db = db;
        }

        // -----------------------------------
        // GET ALL SUSPECTS
        // -----------------------------------
        public async Task<List<Suspect>> GetAllSuspectsAsync()
        {
            var suspects = new List<Suspect>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = "SELECT * FROM suspect_list";

            using var cmd = new MySqlCommand(query, conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                suspects.Add(new Suspect
                {
                    Suspect_ID = reader.GetInt32("Suspect_ID"),
                    Evidence = reader.GetString("Evidence"),
                    Investigation_ID = reader.GetInt32("Investigation_ID")
                });
            }

            return suspects;
        }

        // -----------------------------------
        // GET SUSPECT BY ID
        // -----------------------------------
        public async Task<Suspect?> GetSuspectByIdAsync(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT *
                FROM suspect_list
                WHERE Suspect_ID = @id";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new Suspect
            {
                Suspect_ID = reader.GetInt32("Suspect_ID"),
                Evidence = reader.GetString("Evidence"),
                Investigation_ID = reader.GetInt32("Investigation_ID")
            };
        }

        // -----------------------------------
        // CREATE SUSPECT
        // -----------------------------------
        public async Task<Suspect> CreateSuspectAsync(
            CreateSuspectDto dto)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            int suspectId = new Random().Next(1000, 999999);

            string query = @"
                INSERT INTO suspect_list
                (
                    Suspect_ID,
                    Evidence,
                    Investigation_ID
                )
                VALUES
                (
                    @Suspect_ID,
                    @Evidence,
                    @Investigation_ID
                )";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue(
                "@Suspect_ID",
                suspectId
            );

            cmd.Parameters.AddWithValue(
                "@Evidence",
                dto.Evidence
            );

            cmd.Parameters.AddWithValue(
                "@Investigation_ID",
                dto.Investigation_ID
            );

            await cmd.ExecuteNonQueryAsync();

            return new Suspect
            {
                Suspect_ID = suspectId,
                Evidence = dto.Evidence,
                Investigation_ID = dto.Investigation_ID
            };
        }

        // -----------------------------------
        // UPDATE SUSPECT
        // -----------------------------------
        public async Task<Suspect?> UpdateSuspectAsync(
            int id,
            UpdateSuspectDto dto)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                UPDATE suspect_list
                SET
                    Evidence = @Evidence,
                    Investigation_ID = @Investigation_ID
                WHERE Suspect_ID = @id";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);

            cmd.Parameters.AddWithValue(
                "@Evidence",
                dto.Evidence
            );

            cmd.Parameters.AddWithValue(
                "@Investigation_ID",
                dto.Investigation_ID
            );

            int rows = await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return null;

            return await GetSuspectByIdAsync(id);
        }

        // -----------------------------------
        // DELETE SUSPECT
        // -----------------------------------
        public async Task<bool> DeleteSuspectAsync(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                DELETE FROM suspect_list
                WHERE Suspect_ID = @id";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);

            int rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }
    }
}