using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public class CaseService : ICaseService
    {
        private readonly PoliceDbContext _db;

        public CaseService(PoliceDbContext db)
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
        public void AssignCase(int caseId, String userId)
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
        public List<object> GetMyCases(String officerId)
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

    public async Task<List<Case>> GetAssignedCasesAsync()
        {
            var cases = new List<Case>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
        SELECT Case_ID, Case_Type, Title, Description, Status, OpenDate, CloseDate, Priority
        FROM cases";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                cases.Add(new Case
                {
                    Case_ID = reader.GetInt32(reader.GetOrdinal("Case_ID")),
                    Case_Type = reader.GetString(reader.GetOrdinal("Case_Type")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    OpenDate = reader.GetDateTime(reader.GetOrdinal("OpenDate")),
                    CloseDate = reader.GetDateTime(reader.GetOrdinal("CloseDate")),
                    Priority = reader.IsDBNull(reader.GetOrdinal("Priority"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Priority"))
                });
            }

            return cases;
        }

        public async Task<List<CaseNote>> GetCaseNotesAsync(int caseId)
        {
            var notes = new List<CaseNote>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
        SELECT Note_ID, Case_ID, Note_Text, Created_At
        FROM case_notes
        WHERE Case_ID = @Case_ID";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Case_ID", caseId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                notes.Add(new CaseNote
                {
                    Note_ID = reader.GetInt32(reader.GetOrdinal("Note_ID")),
                    Case_ID = reader.GetInt32(reader.GetOrdinal("Case_ID")),
                    Note_Text = reader.GetString(reader.GetOrdinal("Note_Text")),
                    Created_At = reader.GetDateTime(reader.GetOrdinal("Created_At"))
                });
            }

            return notes;
        }

        public async Task<CaseNote> AddCaseNoteAsync(int caseId, CreateCaseNoteDto dto)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            int noteId = new Random().Next(1000, 999999);
            DateTime createdAt = DateTime.UtcNow;

            string query = @"
        INSERT INTO case_notes
        (Note_ID, Case_ID, Note_Text, Created_At)
        VALUES (@Note_ID, @Case_ID, @Note_Text, @Created_At)";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@Note_ID", noteId);
            cmd.Parameters.AddWithValue("@Case_ID", caseId);
            cmd.Parameters.AddWithValue("@Note_Text", dto.Note_Text);
            cmd.Parameters.AddWithValue("@Created_At", createdAt);

            await cmd.ExecuteNonQueryAsync();

            return new CaseNote
            {
                Note_ID = noteId,
                Case_ID = caseId,
                Note_Text = dto.Note_Text,
                Created_At = createdAt
            };
        }

        public async Task<List<ForensicReport>> GetForensicReportsAsync(int caseId)
        {
            var reports = new List<ForensicReport>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
        SELECT ir.*
        FROM investigation_report ir
        INNER JOIN incident_report rp ON ir.Report_ID = rp.Report_ID
        WHERE rp.Case_ID = @Case_ID";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Case_ID", caseId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reports.Add(new ForensicReport
                {
                    Investigation_ID = reader.GetInt32(reader.GetOrdinal("Investigation_ID")),
                    Investigation_Status = reader.GetString(reader.GetOrdinal("Investigation_Status")),
                    Summary = reader.GetString(reader.GetOrdinal("Summary")),
                    Evidence_analysis = reader.GetString(reader.GetOrdinal("Evidence_analysis")),
                    Suspect_assessment = reader.GetString(reader.GetOrdinal("Suspect_assessment")),
                    Investigative_Conclusions = reader.GetString(reader.GetOrdinal("Investigative_Conclusions")),
                    Evidence = reader.GetString(reader.GetOrdinal("Evidence")),
                    Report_ID = reader.GetInt32(reader.GetOrdinal("Report_ID")),
                    Detective_ID = reader.IsDBNull(reader.GetOrdinal("Detective_ID"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Detective_ID"))
                });
            }

            return reports;
        }
    }
}