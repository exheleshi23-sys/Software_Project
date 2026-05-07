using BCrypt.Net;
using MySql.Data.MySqlClient;
using PoliceAuthBackend.Data;
using PoliceAuthBackend.Models;

namespace PoliceAuthBackend.Services
{
    public class AuthService
    {
        private readonly DbContext _db;

        public AuthService(DbContext db)
        {
            _db = db;
        }

        public User ValidateUser(string username, string password)
        {
            using var conn = _db.GetConnection();

            conn.Open();

            string query =
                "SELECT * FROM user WHERE Name=@username";

            MySqlCommand cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            string storedHash = reader["Password"].ToString();

            bool valid =
                BCrypt.Net.BCrypt.Verify(password, storedHash);

            if (!valid)
                return null;

            return new User
            {
                Id = Convert.ToInt32(reader["User_ID"]),
                Email = reader["Email"].ToString()
            };
        }

        public string GenerateOtp()
        {
            return new Random()
                .Next(100000, 999999)
                .ToString();
        }

        public void SaveOtp(int userId, string code)
        {
            using var conn = _db.GetConnection();

            conn.Open();

            string query =
                @"INSERT INTO login_otp
                (User_ID, Code, Expiry)
                VALUES
                (@uid, @code, @expiry)";

            MySqlCommand cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@uid", userId);

            cmd.Parameters.AddWithValue("@code", code);

            cmd.Parameters.AddWithValue(
                "@expiry",
                DateTime.Now.AddMinutes(5)
            );

            cmd.ExecuteNonQuery();
        }

        public bool VerifyOtp(int userId, string code)
        {
            using var conn = _db.GetConnection();

            conn.Open();

            string query =
                @"SELECT * FROM login_otp
                  WHERE User_ID=@uid
                  AND Code=@code
                  AND IsUsed=0
                  AND Expiry > NOW()";

            MySqlCommand cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@uid", userId);

            cmd.Parameters.AddWithValue("@code", code);

            using var reader = cmd.ExecuteReader();

            return reader.Read();
        }
    }
}