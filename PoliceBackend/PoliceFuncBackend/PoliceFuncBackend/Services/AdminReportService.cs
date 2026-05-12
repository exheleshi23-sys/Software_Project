using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;

namespace PoliceFuncBackend.Services
{
    public class AdminReportService
    {
        private readonly DbContext _db;

        public AdminReportService(DbContext db)
        {
            _db = db;
        }

        public List<object> ExportData()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT 
                u.User_ID,
                u.Name,
                u.Surname,
                r.Role_Name,
                d.Department_Name
                FROM user u
                JOIN roles r ON u.Role_ID = r.Role_ID
                JOIN departments d ON u.Department_ID = d.Department_ID
            ", conn);

            var reader = cmd.ExecuteReader();

            var list = new List<object>();

            while (reader.Read())
            {
                list.Add(new
                {
                    Id = reader["User_ID"],
                    Name = reader["Name"],
                    Surname = reader["Surname"],
                    Role = reader["Role_Name"],
                    Department = reader["Department_Name"]
                });
            }

            return list;
        }
    }
}