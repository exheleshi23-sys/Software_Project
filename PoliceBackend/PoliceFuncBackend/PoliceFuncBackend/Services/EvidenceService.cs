using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;
using System.Text.Json;

namespace PoliceFuncBackend.Services
{
    public class EvidenceService
    {
        private readonly DbContext _db;

        public EvidenceService(DbContext db)
        {
            _db = db;
        }

        // GET all by case_id
        public List<object> GetEvidence(int? caseId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = "SELECT * FROM evidence";

            if (caseId.HasValue)
                query += " WHERE case_id=@caseId";

            var cmd = new MySqlCommand(query, conn);

            if (caseId.HasValue)
                cmd.Parameters.AddWithValue("@caseId", caseId);

            var reader = cmd.ExecuteReader();

            var list = new List<object>();

            while (reader.Read())
            {
                list.Add(new
                {
                    Id = reader["id"],
                    EvidenceNumber = reader["evidence_number"],
                    CaseId = reader["case_id"],
                    Type = reader["evidence_type"],
                    Status = reader["status"]
                });
            }

            return list;
        }

        // GET by id
        public object? GetById(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM evidence WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            var reader = cmd.ExecuteReader();

            if (!reader.Read()) return null;

            return new
            {
                Id = reader["id"],
                EvidenceNumber = reader["evidence_number"],
                CaseId = reader["case_id"],
                Type = reader["evidence_type"],
                Description = reader["description"],
                Status = reader["status"],
                Chain = reader["chain_of_custody"]
            };
        }

        // POST create evidence
        public void CreateEvidence(EvidenceCreateDto dto)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = @"
                INSERT INTO evidence
                (evidence_number, case_id, evidence_type, description, collection_date, collected_by)
                VALUES
                (@num, @case, @type, @desc, @date, @collectedBy)";

            var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@num", dto.Evidence_Number);
            cmd.Parameters.AddWithValue("@case", dto.Case_ID);
            cmd.Parameters.AddWithValue("@type", dto.Evidence_Type);
            cmd.Parameters.AddWithValue("@desc", dto.Description);
            cmd.Parameters.AddWithValue("@date", dto.Collection_Date);
            cmd.Parameters.AddWithValue("@collectedBy", dto.Collected_By);

            cmd.ExecuteNonQuery();
        }

        // PUT transfer (chain of custody)
        public void TransferEvidence(int id, EvidenceTransferDto dto)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            // 1. Get current chain
            var getCmd = new MySqlCommand("SELECT chain_of_custody FROM evidence WHERE id=@id", conn);
            getCmd.Parameters.AddWithValue("@id", id);

            var current = getCmd.ExecuteScalar()?.ToString() ?? "[]";

            var chain = JsonSerializer.Deserialize<List<object>>(current)
                        ?? new List<object>();

            // 2. Append new action
            chain.Add(new
            {
                user = dto.User_ID,
                action = dto.Action,
                time = DateTime.UtcNow
            });

            string updated = JsonSerializer.Serialize(chain);

            // 3. Update DB
            var updateCmd = new MySqlCommand(
                "UPDATE evidence SET chain_of_custody=@chain WHERE id=@id", conn);

            updateCmd.Parameters.AddWithValue("@chain", updated);
            updateCmd.Parameters.AddWithValue("@id", id);

            updateCmd.ExecuteNonQuery();
        }
    }
}