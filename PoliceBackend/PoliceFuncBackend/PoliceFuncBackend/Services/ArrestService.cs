using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;

namespace PoliceFuncBackend.Services
{
    public class ArrestService
    {
        private readonly DbContext _db;

        public ArrestService(DbContext db)
        {
            _db = db;
        }

        // GET /api/arrests
        public List<object> GetAll()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM arrests", conn);
            var reader = cmd.ExecuteReader();

            var list = new List<object>();

            while (reader.Read())
            {
                list.Add(new
                {
                    Id = reader["id"],
                    ArrestNumber = reader["arrest_number"],
                    Status = reader["status"],
                    CaseId = reader["case_id"],
                    SuspectId = reader["suspect_id"]
                });
            }

            return list;
        }

        // GET /api/arrests/:id
        public object? GetById(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM arrests WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            var reader = cmd.ExecuteReader();

            if (!reader.Read()) return null;

            return new
            {
                Id = reader["id"],
                ArrestNumber = reader["arrest_number"],
                SuspectId = reader["suspect_id"],
                CaseId = reader["case_id"],
                OfficerId = reader["arresting_officer_id"],
                ArrestDate = reader["arrest_date"],
                Charges = reader["charges"],
                Status = reader["status"]
            };
        }

        // POST /api/arrests
        public void Create(ArrestCreateDto dto)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                INSERT INTO arrests
                (arrest_number, suspect_id, case_id, arresting_officer_id, arrest_date, charges, status)
                VALUES
                (@num, @suspect, @case, @officer, @date, @charges, @status)", conn);

            cmd.Parameters.AddWithValue("@num", dto.Arrest_Number);
            cmd.Parameters.AddWithValue("@suspect", dto.Suspect_ID);
            cmd.Parameters.AddWithValue("@case", dto.Case_ID);
            cmd.Parameters.AddWithValue("@officer", dto.Arresting_Officer_ID);
            cmd.Parameters.AddWithValue("@date", dto.Arrest_Date);
            cmd.Parameters.AddWithValue("@charges", dto.Charges);
            cmd.Parameters.AddWithValue("@status", dto.Status ?? "detained");

            cmd.ExecuteNonQuery();
        }

        // PUT /api/arrests/:id/status
        public void UpdateStatus(int id, string status)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "UPDATE arrests SET status=@status WHERE id=@id", conn);

            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }
    }
}