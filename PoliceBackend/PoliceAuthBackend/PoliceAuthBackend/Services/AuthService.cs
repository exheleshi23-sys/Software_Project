using BCrypt.Net;
using MySql.Data.MySqlClient;
using PoliceAuthBackend.Data;
using PoliceAuthBackend.Models;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace PoliceAuthBackend.Services
{
    public class AuthService
    {
        private readonly DbContext _db;

        public AuthService(DbContext db)
        {
            _db = db;
        }

        public User ValidateUser(
            string username,
            string password)
        {
            using var conn = _db.GetConnection();

            conn.Open();

            string query =
                "SELECT * FROM user WHERE Name=@username";

            MySqlCommand cmd =
                new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue(
                "@username",
                username
            );

            using var reader =
                cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            string storedHash =
                reader["Password"].ToString();

            bool valid =
                BCrypt.Net.BCrypt.Verify(
                    password,
                    storedHash
                );

            if (!valid)
                return null;

            return new User
            {
                Id = Convert.ToInt32(
                    reader["User_ID"]
                ),

                Name = reader["Name"].ToString(),

                Email = reader["Email"].ToString(),

                Password =
                    reader["Password"].ToString(),

                Role_ID = Convert.ToInt32(
                    reader["Role_ID"]
                ),

                Department_ID = Convert.ToInt32(
                    reader["Department_ID"]
                )
            };
        }

        public User GetUserById(int userId)
        {
            using var conn = _db.GetConnection();

            conn.Open();

            string query =
                "SELECT * FROM user WHERE User_ID=@uid";

            MySqlCommand cmd =
                new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue(
                "@uid",
                userId
            );

            using var reader =
                cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new User
            {
                Id = Convert.ToInt32(
                    reader["User_ID"]
                ),

                Name = reader["Name"].ToString(),

                Email = reader["Email"].ToString(),

                Role_ID = Convert.ToInt32(
                    reader["Role_ID"]
                ),

                Department_ID = Convert.ToInt32(
                    reader["Department_ID"]
                )
            };
        }

        public User GetUserByEmailOrUsername(
            string input)
        {
            using var conn = _db.GetConnection();

            conn.Open();

            string query =
                @"SELECT * FROM user
                  WHERE Email=@input
                  OR Name=@input";

            MySqlCommand cmd =
                new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue(
                "@input",
                input
            );

            using var reader =
                cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new User
            {
                Id = Convert.ToInt32(
                    reader["User_ID"]
                ),

                Name = reader["Name"].ToString(),

                Email = reader["Email"].ToString(),

                Role_ID = Convert.ToInt32(
                    reader["Role_ID"]
                ),

                Department_ID = Convert.ToInt32(
                    reader["Department_ID"]
                )
            };
        }

        public string GenerateOtp()
        {
            return new Random()
                .Next(100000, 999999)
                .ToString();
        }

        public void SaveOtp(
            int userId,
            string code)
        {
            using var conn = _db.GetConnection();

            conn.Open();

            string query =
                @"INSERT INTO login_otp
                (User_ID, Code, Expiry)
                VALUES
                (@uid, @code, @expiry)";

            MySqlCommand cmd =
                new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue(
                "@uid",
                userId
            );

            cmd.Parameters.AddWithValue(
                "@code",
                code
            );

            cmd.Parameters.AddWithValue(
                "@expiry",
                DateTime.Now.AddMinutes(5)
            );

            cmd.ExecuteNonQuery();
        }

        public bool VerifyOtp(
            int userId,
            string code)
        {
            using var conn = _db.GetConnection();

            conn.Open();

            string query =
                @"SELECT * FROM login_otp
                  WHERE User_ID=@uid
                  AND Code=@code
                  AND IsUsed=0
                  AND Expiry > NOW()";

            MySqlCommand cmd =
                new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue(
                "@uid",
                userId
            );

            cmd.Parameters.AddWithValue(
                "@code",
                code
            );

            using var reader =
                cmd.ExecuteReader();

            bool valid = reader.Read();

            reader.Close();

            if (valid)
            {
                string updateQuery =
                    @"UPDATE login_otp
                      SET IsUsed=1
                      WHERE User_ID=@uid
                      AND Code=@code";

                MySqlCommand updateCmd =
                    new MySqlCommand(
                        updateQuery,
                        conn
                    );

                updateCmd.Parameters.AddWithValue(
                    "@uid",
                    userId
                );

                updateCmd.Parameters.AddWithValue(
                    "@code",
                    code
                );

                updateCmd.ExecuteNonQuery();
            }

            return valid;
        }

        public void UpdatePassword(
            int userId,
            string newPassword)
        {
            using var conn = _db.GetConnection();

            conn.Open();

            string hash =
                BCrypt.Net.BCrypt.HashPassword(
                    newPassword
                );

            string query =
                @"UPDATE user
                  SET Password=@pass
                  WHERE User_ID=@uid";

            MySqlCommand cmd =
                new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue(
                "@pass",
                hash
            );

            cmd.Parameters.AddWithValue(
                "@uid",
                userId
            );

            cmd.ExecuteNonQuery();
        }

        public string GenerateJwtToken(
            User user,
            IConfiguration config)
        {
            string roleName =
                GetRoleName(user.Role_ID);

            var claims = new[]
            {
                new Claim(
                    "userId",
                    user.Id.ToString()
                ),

                new Claim(
                    "departmentId",
                    user.Department_ID.ToString()
                ),

                new Claim(
                    ClaimTypes.Role,
                    roleName
                ),

                new Claim(
                    JwtRegisteredClaimNames.Email,
                    user.Email
                ),

                new Claim(
                    JwtRegisteredClaimNames.UniqueName,
                    user.Name
                )
            };

            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        config["Jwt:Key"]
                    )
                );

            var creds =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );

            var token =
                new JwtSecurityToken(
                    issuer:
                        config["Jwt:Issuer"],

                    audience:
                        config["Jwt:Audience"],

                    claims: claims,

                    expires:
                        DateTime.Now.AddHours(24),

                    signingCredentials:
                        creds
                );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        private string GetRoleName(int roleId)
        {
            return roleId switch
            {
                1 => "Admin",
                2 => "Officer",
                3 => "Detective",
                4 => "Forensic",
                5 => "Traffic",
                _ => "Citizen"
            };
        }
    }
}