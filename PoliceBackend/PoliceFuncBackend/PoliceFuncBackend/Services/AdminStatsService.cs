using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;

namespace PoliceFuncBackend.Services
{
    public class AdminStatsService
    {
        private readonly DbContext _db;

        public AdminStatsService(DbContext db)
        {
            _db = db;
        }

        // -------------------------
        // KPI OVERVIEW
        // -------------------------
        public object GetOverview()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT 
                (SELECT COUNT(*) FROM user) AS Users,
                (SELECT COUNT(*) FROM cases) AS Cases,
                (SELECT COUNT(*) FROM traffic_fine) AS Fines,
                (SELECT COUNT(*) FROM incident_report) AS Incidents;
            ", conn);

            var reader = cmd.ExecuteReader();
            reader.Read();

            return new
            {
                Users = reader["Users"],
                Cases = reader["Cases"],
                Fines = reader["Fines"],
                Incidents = reader["Incidents"]
            };
        }

        // -------------------------
        // CASES BY TYPE
        // -------------------------
        public List<object> GetCasesByType()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT Case_Type, COUNT(*) AS Total
                FROM cases
                GROUP BY Case_Type
            ", conn);

            var reader = cmd.ExecuteReader();

            var list = new List<object>();

            while (reader.Read())
            {
                list.Add(new
                {
                    Type = reader["Case_Type"],
                    Total = reader["Total"]
                });
            }

            return list;
        }

        // -------------------------
        // MONTHLY ACTIVITY
        // -------------------------
        public List<object> GetMonthlyActivity()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT 
                MONTH(OpenDate) AS Month,
                COUNT(*) AS Total
                FROM cases
                GROUP BY MONTH(OpenDate)
            ", conn);

            var reader = cmd.ExecuteReader();

            var list = new List<object>();

            while (reader.Read())
            {
                list.Add(new
                {
                    Month = reader["Month"],
                    Total = reader["Total"]
                });
            }

            return list;
        }
    }
}