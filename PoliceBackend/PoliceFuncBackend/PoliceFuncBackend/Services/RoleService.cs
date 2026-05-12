using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;

namespace PoliceFuncBackend.Services
{
    public class RoleService
    {
        private readonly DbContext _db;

        public RoleService(DbContext db)
        {
            _db = db;
        }

        public string GetRoleName(int roleId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = "SELECT Role_Name FROM roles WHERE Role_ID=@id";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", roleId);

            var result = cmd.ExecuteScalar();
            return result?.ToString();
        }
    }
}