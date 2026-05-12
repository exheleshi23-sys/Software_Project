using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;

namespace PoliceFuncBackend.Services
{
    public class IncidentService
    {
        private readonly DbContext _db;

        public IncidentService(DbContext db)
        {
            _db = db;
        }

        // GET INCIDENTS (FILTER)
        public List<object> GetIncidents(string type = null)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = "SELECT * FROM incident_report WHERE 1=1";

            if (!string.IsNullOrEmpty(type))
                query += " AND Type=@type";

            var cmd = new MySqlCommand(query, conn);

            if (!string.IsNullOrEmpty(type))
                cmd.Parameters.AddWithValue("@type", type);

            var reader = cmd.ExecuteReader();

            var list = new List<object>();

            while (reader.Read())
            {
                list.Add(new
                {
                    Id = reader["Report_ID"],
                    Type = reader["Type"],
                    Location = reader["Location"],
                    Date = reader["Date"],
                    Case_ID = reader["Case_ID"]
                });
            }

            return list;
        }

        // GET BY ID
        public object GetIncidentById(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "SELECT * FROM incident_report WHERE Report_ID=@id", conn);

            cmd.Parameters.AddWithValue("@id", id);

            var reader = cmd.ExecuteReader();

            if (!reader.Read()) return null;

            return new
            {
                Id = reader["Report_ID"],
                Type = reader["Type"],
                Location = reader["Location"],
                Description = reader["Description"],
                Case_ID = reader["Case_ID"]
            };
        }

        // CREATE INCIDENT
        public void CreateIncident(IncidentDto dto)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = @"INSERT INTO incident_report
            (Report_ID, Type, Location, Description, Date, Time, Arrest_Record, Officer_ID, Case_ID)
            VALUES
            (@id,@type,@loc,@desc,@date,@time,@arrest,@officer,@caseId)";

            var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", dto.Report_ID);
            cmd.Parameters.AddWithValue("@type", dto.Type);
            cmd.Parameters.AddWithValue("@loc", dto.Location);
            cmd.Parameters.AddWithValue("@desc", dto.Description);
            cmd.Parameters.AddWithValue("@date", dto.Date);
            cmd.Parameters.AddWithValue("@time", dto.Time);
            cmd.Parameters.AddWithValue("@arrest", dto.Arrest_Record);
            cmd.Parameters.AddWithValue("@officer", dto.Officer_ID);
            cmd.Parameters.AddWithValue("@caseId", (object?)dto.Case_ID ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        // UPDATE INCIDENT
        public void UpdateIncident(int id, UpdateIncidentDto dto)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = @"UPDATE incident_report
            SET Type=@type,
                Location=@loc,
                Description=@desc,
                Date=@date,
                Time=@time,
                Arrest_Record=@arrest
            WHERE Report_ID=@id";

            var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@type", dto.Type);
            cmd.Parameters.AddWithValue("@loc", dto.Location);
            cmd.Parameters.AddWithValue("@desc", dto.Description);
            cmd.Parameters.AddWithValue("@date", dto.Date);
            cmd.Parameters.AddWithValue("@time", dto.Time);
            cmd.Parameters.AddWithValue("@arrest", dto.Arrest_Record);

            cmd.ExecuteNonQuery();
        }

        // LINK CASE
        public void LinkCase(int id, int caseId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "UPDATE incident_report SET Case_ID=@caseId WHERE Report_ID=@id", conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@caseId", caseId);

            cmd.ExecuteNonQuery();
        }
    }
}