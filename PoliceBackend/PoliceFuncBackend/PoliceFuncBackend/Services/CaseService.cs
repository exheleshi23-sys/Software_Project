using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;

namespace PoliceFuncBackend.Services
{
    public class CaseService
    {
        private readonly DbContext _db;

        public CaseService(DbContext db)
        {
            _db = db;
        }

        // GET ALL CASES (FILTERS)
        public List<object> GetCases(string status = null, string priority = null)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = "SELECT * FROM cases WHERE 1=1";

            if (!string.IsNullOrEmpty(status))
                query += " AND Status=@status";

            if (!string.IsNullOrEmpty(priority))
                query += " AND Priority=@priority";

            var cmd = new MySqlCommand(query, conn);

            if (!string.IsNullOrEmpty(status))
                cmd.Parameters.AddWithValue("@status", status);

            if (!string.IsNullOrEmpty(priority))
                cmd.Parameters.AddWithValue("@priority", priority);

            var reader = cmd.ExecuteReader();

            var list = new List<object>();

            while (reader.Read())
            {
                list.Add(new
                {
                    Id = reader["Case_ID"],
                    Type = reader["Case_Type"],
                    Title = reader["Title"],
                    Status = reader["Status"],
                    Priority = reader["Priority"]
                });
            }

            return list;
        }

        // GET CASE BY ID
        public object GetCaseById(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM cases WHERE Case_ID=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            var reader = cmd.ExecuteReader();

            if (!reader.Read()) return null;

            return new
            {
                Id = reader["Case_ID"],
                Type = reader["Case_Type"],
                Title = reader["Title"],
                Description = reader["Description"],
                Status = reader["Status"],
                Priority = reader["Priority"]
            };
        }

        // CREATE CASE
        public void CreateCase(CaseDto dto)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = @"INSERT INTO cases 
                (Case_ID, Case_Type, Title, Description, Status, Priority, OpenDate, CloseDate)
                VALUES
                (@id,@type,@title,@desc,@status,@priority,CURDATE(),CURDATE())";

            var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", dto.Case_ID);
            cmd.Parameters.AddWithValue("@type", dto.Case_Type);
            cmd.Parameters.AddWithValue("@title", dto.Title);
            cmd.Parameters.AddWithValue("@desc", dto.Description);
            cmd.Parameters.AddWithValue("@status", dto.Status);
            cmd.Parameters.AddWithValue("@priority", dto.Priority);

            cmd.ExecuteNonQuery();
        }

        // UPDATE CASE
        public void UpdateCase(int id, UpdateCaseDto dto)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = @"UPDATE cases 
                SET Case_Type=@type,
                    Title=@title,
                    Description=@desc,
                    Status=@status,
                    Priority=@priority
                WHERE Case_ID=@id";

            var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@type", dto.Case_Type);
            cmd.Parameters.AddWithValue("@title", dto.Title);
            cmd.Parameters.AddWithValue("@desc", dto.Description);
            cmd.Parameters.AddWithValue("@status", dto.Status);
            cmd.Parameters.AddWithValue("@priority", dto.Priority);

            cmd.ExecuteNonQuery();
        }

        // UPDATE STATUS ONLY
        public void UpdateStatus(int id, string status)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "UPDATE cases SET Status=@status WHERE Case_ID=@id", conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@status", status);

            cmd.ExecuteNonQuery();
        }

        // ASSIGN CASE
        public void AssignCase(int caseId, int userId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                "INSERT INTO case_assignment (Assignment_ID, AssignmentDate, AssignmentStatus, Case_ID) VALUES (@aid, CURDATE(), 'assigned', @cid)", conn);

            cmd.Parameters.AddWithValue("@aid", new Random().Next(1000, 9999));
            cmd.Parameters.AddWithValue("@cid", caseId);

            cmd.ExecuteNonQuery();
        }

        // CASES FOR OFFICER
        public List<object> GetMyCases(int officerId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = @"
                SELECT c.*
                FROM cases c
                JOIN case_assignment ca ON c.Case_ID = ca.Case_ID
                WHERE ca.AssignmentStatus='assigned'
            ";

            var cmd = new MySqlCommand(query, conn);
            var reader = cmd.ExecuteReader();

            var list = new List<object>();

            while (reader.Read())
            {
                list.Add(new
                {
                    Id = reader["Case_ID"],
                    Title = reader["Title"],
                    Status = reader["Status"]
                });
            }

            return list;
        }
    }
}