using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;

namespace PoliceFuncBackend.Services
{
    public class DepartmentService
    {
        private readonly PoliceDbContext _db;

        public DepartmentService(PoliceDbContext db)
        {
            _db = db;
        }

        // GET ALL DEPARTMENTS
        public List<object> GetDepartments()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM departments", conn);
            var reader = cmd.ExecuteReader();

            var list = new List<object>();

            while (reader.Read())
            {
                list.Add(new
                {
                    Id = reader["Department_ID"],
                    Name = reader["Department_Name"],
                    Description = reader["Description"]
                });
            }

            return list;
        }

        // CREATE DEPARTMENT
        public void CreateDepartment(DepartmentDto dto)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = @"INSERT INTO departments 
                            (Department_ID, Department_Name, Description)
                            VALUES (@id, @name, @desc)";

            var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", dto.Department_ID);
            cmd.Parameters.AddWithValue("@name", dto.Department_Name);
            cmd.Parameters.AddWithValue("@desc", dto.Description);

            cmd.ExecuteNonQuery();
        }

        // UPDATE DEPARTMENT
        public void UpdateDepartment(int id, UpdateDepartmentDto dto)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = @"UPDATE departments 
                             SET Department_Name=@name,
                                 Description=@desc
                             WHERE Department_ID=@id";

            var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", dto.Department_Name);
            cmd.Parameters.AddWithValue("@desc", dto.Description);

            cmd.ExecuteNonQuery();
        }

        // DELETE DEPARTMENT
        public void DeleteDepartment(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = "DELETE FROM departments WHERE Department_ID=@id";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }
    }
}