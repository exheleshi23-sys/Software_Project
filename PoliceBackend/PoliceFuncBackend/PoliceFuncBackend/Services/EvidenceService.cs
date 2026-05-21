using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Models;
using System.Data;
using System.Text.Json;

namespace PoliceFuncBackend.Services
{
    public class EvidenceService : IEvidenceService
    {
        private readonly PoliceDbContext _db;

        public EvidenceService(PoliceDbContext db)
        {
            _db = db;
        }

        // -----------------------------------
        // GET ALL EVIDENCE
        // -----------------------------------
        public List<object> GetEvidence(int? caseId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = "SELECT * FROM evidence";

            if (caseId.HasValue)
                query += " WHERE case_id=@caseId";

            using var cmd = new MySqlCommand(query, conn);

            if (caseId.HasValue)
                cmd.Parameters.AddWithValue("@caseId", caseId);

            using var reader = cmd.ExecuteReader();

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

        // -----------------------------------
        // GET EVIDENCE BY ID
        // -----------------------------------
        public object? GetById(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(
                "SELECT * FROM evidence WHERE id=@id",
                conn
            );

            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

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

        // -----------------------------------
        // CREATE EVIDENCE
        // -----------------------------------
        public void CreateEvidence(EvidenceCreateDto dto)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = @"
                INSERT INTO evidence
                (
                    evidence_number,
                    case_id,
                    evidence_type,
                    description,
                    collection_date,
                    collected_by
                )
                VALUES
                (
                    @num,
                    @case,
                    @type,
                    @desc,
                    @date,
                    @collectedBy
                )";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@num", dto.Evidence_Number);
            cmd.Parameters.AddWithValue("@case", dto.Case_ID);
            cmd.Parameters.AddWithValue("@type", dto.Evidence_Type);
            cmd.Parameters.AddWithValue("@desc", dto.Description);
            cmd.Parameters.AddWithValue("@date", dto.Collection_Date);
            cmd.Parameters.AddWithValue("@collectedBy", dto.Collected_By);

            cmd.ExecuteNonQuery();
        }

        // -----------------------------------
        // TRANSFER EVIDENCE
        // -----------------------------------
        public void TransferEvidence(int id, EvidenceTransferDto dto)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var getCmd = new MySqlCommand(
                "SELECT chain_of_custody FROM evidence WHERE id=@id",
                conn
            );

            getCmd.Parameters.AddWithValue("@id", id);

            var current =
                getCmd.ExecuteScalar()?.ToString() ?? "[]";

            var chain =
                JsonSerializer.Deserialize<List<object>>(current)
                ?? new List<object>();

            chain.Add(new
            {
                user = dto.User_ID,
                action = dto.Action,
                time = DateTime.UtcNow
            });

            string updated =
                JsonSerializer.Serialize(chain);

            var updateCmd = new MySqlCommand(
                "UPDATE evidence SET chain_of_custody=@chain WHERE id=@id",
                conn
            );

            updateCmd.Parameters.AddWithValue("@chain", updated);
            updateCmd.Parameters.AddWithValue("@id", id);

            updateCmd.ExecuteNonQuery();
        }

        // -----------------------------------
        // GET EVIDENCE BY CASE ID
        // -----------------------------------
        public async Task<List<Evidence>> GetEvidenceByCaseIdAsync(int caseId)
        {
            var evidenceList = new List<Evidence>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT *
                FROM evidence
                WHERE case_id = @case_id";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@case_id", caseId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                evidenceList.Add(new Evidence
                {
                    id = reader.GetInt32("id"),
                    evidence_number = reader.GetString("evidence_number"),
                    case_id = reader.GetInt32("case_id"),
                    evidence_type = reader.IsDBNull(reader.GetOrdinal("evidence_type"))
                        ? null
                        : reader.GetString("evidence_type"),
                    description = reader.GetString("description"),
                    collection_date = reader.GetDateTime("collection_date"),
                    status = reader.IsDBNull(reader.GetOrdinal("status"))
                        ? null
                        : reader.GetString("status"),
                    collected_by = reader.IsDBNull(reader.GetOrdinal("collected_by"))
                        ? null
                        : reader.GetString("collected_by"),
                    analyzed_by = reader.IsDBNull(reader.GetOrdinal("analyzed_by"))
                        ? null
                        : reader.GetString("analyzed_by"),
                    chain_of_custody = reader.IsDBNull(reader.GetOrdinal("chain_of_custody"))
                        ? null
                        : reader.GetString("chain_of_custody")
                });
            }

            return evidenceList;
        }

        // -----------------------------------
        // GET EVIDENCE QUEUE
        // -----------------------------------
        public async Task<List<Evidence>> GetEvidenceQueueAsync()
        {
            var evidenceList = new List<Evidence>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT *
                FROM evidence
                WHERE status='collected'
                   OR status='in_analysis'";

            using var cmd = new MySqlCommand(query, conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                evidenceList.Add(new Evidence
                {
                    id = reader.GetInt32("id"),
                    evidence_number = reader.GetString("evidence_number"),
                    case_id = reader.GetInt32("case_id"),
                    evidence_type = reader.IsDBNull(reader.GetOrdinal("evidence_type"))
                        ? null
                        : reader.GetString("evidence_type"),
                    description = reader.GetString("description"),
                    collection_date = reader.GetDateTime("collection_date"),
                    status = reader.IsDBNull(reader.GetOrdinal("status"))
                        ? null
                        : reader.GetString("status"),
                    collected_by = reader.IsDBNull(reader.GetOrdinal("collected_by"))
                        ? null
                        : reader.GetString("collected_by"),
                    analyzed_by = reader.IsDBNull(reader.GetOrdinal("analyzed_by"))
                        ? null
                        : reader.GetString("analyzed_by"),
                    chain_of_custody = reader.IsDBNull(reader.GetOrdinal("chain_of_custody"))
                        ? null
                        : reader.GetString("chain_of_custody")
                });
            }

            return evidenceList;
        }

        // -----------------------------------
        // ASSIGN ANALYST
        // -----------------------------------
        public async Task<Evidence?> AssignAnalystAsync(
            int id,
            AssignAnalystDto dto)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                UPDATE evidence
                SET analyzed_by=@analyst,
                    status='in_analysis'
                WHERE id=@id";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue(
                "@analyst",
                dto.Analyst_ID
            );

            int rows =
                await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return null;

            return await GetEvidenceByIdAsync(id);
        }

        // -----------------------------------
        // UPDATE STATUS
        // -----------------------------------
        public async Task<Evidence?> UpdateStatusAsync(
            int id,
            UpdateEvidenceStatusDto dto)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                UPDATE evidence
                SET status=@status
                WHERE id=@id";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@status", dto.Status);

            int rows =
                await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return null;

            return await GetEvidenceByIdAsync(id);
        }

        // -----------------------------------
        // GET CHAIN OF CUSTODY
        // -----------------------------------
        public async Task<string?> GetChainOfCustodyAsync(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT chain_of_custody
                FROM evidence
                WHERE id=@id";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);

            var result =
                await cmd.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value)
                return null;

            return result.ToString();
        }

        // -----------------------------------
        // ADD CHAIN OF CUSTODY ENTRY
        // -----------------------------------
        public async Task<Evidence?> AddChainOfCustodyAsync(
            int id,
            ChainOfCustodyDto dto)
        {
            string? existing =
                await GetChainOfCustodyAsync(id);

            string newEntry =
                $"{DateTime.UtcNow}: {dto.Entry}";

            string updatedChain =
                string.IsNullOrWhiteSpace(existing)
                    ? newEntry
                    : existing + "\n" + newEntry;

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                UPDATE evidence
                SET chain_of_custody=@chain
                WHERE id=@id";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@chain", updatedChain);

            int rows =
                await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return null;

            return await GetEvidenceByIdAsync(id);
        }

        // -----------------------------------
        // PRIVATE HELPER
        // -----------------------------------
        private async Task<Evidence?> GetEvidenceByIdAsync(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT *
                FROM evidence
                WHERE id=@id";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);

            using var reader =
                await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new Evidence
            {
                id = reader.GetInt32("id"),
                evidence_number = reader.GetString("evidence_number"),
                case_id = reader.GetInt32("case_id"),
                evidence_type = reader.IsDBNull(reader.GetOrdinal("evidence_type"))
                    ? null
                    : reader.GetString("evidence_type"),
                description = reader.GetString("description"),
                collection_date = reader.GetDateTime("collection_date"),
                status = reader.IsDBNull(reader.GetOrdinal("status"))
                    ? null
                    : reader.GetString("status"),
                collected_by = reader.IsDBNull(reader.GetOrdinal("collected_by"))
                    ? null
                    : reader.GetString("collected_by"),
                analyzed_by = reader.IsDBNull(reader.GetOrdinal("analyzed_by"))
                    ? null
                    : reader.GetString("analyzed_by"),
                chain_of_custody = reader.IsDBNull(reader.GetOrdinal("chain_of_custody"))
                    ? null
                    : reader.GetString("chain_of_custody")
            };
        }
    }
}