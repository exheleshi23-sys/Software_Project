using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;

namespace PoliceFuncBackend.Services
{
    public class AdminLogService
    {
        private readonly PoliceDbContext _db;

        public AdminLogService(PoliceDbContext db)
        {
            _db = db;
        }

        public List<object> GetLogs()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT * FROM system_logs
                ORDER BY Created_At DESC
            ", conn);

            var reader = cmd.ExecuteReader();

            var list = new List<object>();

            while (reader.Read())
            {
                list.Add(new
                {
                    Id = reader["Log_ID"],
                    User = reader["User_ID"],
                    Action = reader["Action"],
                    Table = reader["Table_Affected"],
                    Time = reader["Created_At"]
                });
            }

            return list;
        }
    }
}