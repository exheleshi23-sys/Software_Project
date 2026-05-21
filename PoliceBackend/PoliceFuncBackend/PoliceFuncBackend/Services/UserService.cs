using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.DTOs;

namespace PoliceFuncBackend.Services
{
    public class UserService
    {
        private readonly PoliceDbContext _db;

        public UserService(PoliceDbContext db)
        {
            _db = db;
        }

        public List<object> GetUsers()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM user", conn);
            var reader = cmd.ExecuteReader();

            var users = new List<object>();

            while (reader.Read())
            {
                users.Add(new
                {
                    Id = reader["User_ID"],
                    Name = reader["Name"],
                    Surname = reader["Surname"],
                    Email = reader["Email"],
                    RoleId = reader["Role_ID"]
                });
            }

            return users;
        }

        public object GetUserById(String id) {
            using var conn = _db.GetConnection();
            conn.Open();

                string query = "SELECT * FROM user WHERE User_ID=@id";

                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = cmd.ExecuteReader();

                if (!reader.Read()) return null;

                return new {
                    Id = reader["User_ID"],
                    Name = reader["Name"],
                    Surname = reader["Surname"],
                    Email = reader["Email"],
                    Role_ID = reader["Role_ID"],
                    Status = reader["Status"]
                    };
        }

        public void UpdateUser(String id, UpdateUserDto dto) {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = @"UPDATE user 
                     SET Name=@name,
                         Surname=@surname,
                         Email=@email,
                         Phone_Number=@phone,
                         Address=@address
                     WHERE User_ID=@id";

            var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", dto.Name);
            cmd.Parameters.AddWithValue("@surname", dto.Surname);
            cmd.Parameters.AddWithValue("@email", dto.Email);
            cmd.Parameters.AddWithValue("@phone", dto.Phone_Number);
            cmd.Parameters.AddWithValue("@address", dto.Address);

            cmd.ExecuteNonQuery();
        }

        public void UpdateStatus(String id, string status) {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = "UPDATE user SET Status=@status WHERE User_ID=@id";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@status", status);

            cmd.ExecuteNonQuery();
        }

        public void UpdateRole(String id, int roleId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = "UPDATE user SET Role_ID=@role WHERE User_ID=@id";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@role", roleId);

            cmd.ExecuteNonQuery();
        }

        public void CreateUser(UserDto dto)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string query = @"INSERT INTO user
            (User_ID, Name, Surname, Email, Password, Phone_Number, Address, Birth_Date, Role_ID, Department_ID)
            VALUES
            (@id,@name,@surname,@email,@password,@phone,@address,@birth,@role,@dept)";

            var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", dto.User_ID);
            cmd.Parameters.AddWithValue("@name", dto.Name);
            cmd.Parameters.AddWithValue("@surname", dto.Surname);
            cmd.Parameters.AddWithValue("@email", dto.Email);
            cmd.Parameters.AddWithValue("@password", dto.Password);
            cmd.Parameters.AddWithValue("@phone", dto.Phone_Number);
            cmd.Parameters.AddWithValue("@address", dto.Address);
            cmd.Parameters.AddWithValue("@birth", dto.Birth_Date);
            cmd.Parameters.AddWithValue("@role", dto.Role_ID);
            cmd.Parameters.AddWithValue("@dept", dto.Department_ID);

            cmd.ExecuteNonQuery();
        }
    }
}