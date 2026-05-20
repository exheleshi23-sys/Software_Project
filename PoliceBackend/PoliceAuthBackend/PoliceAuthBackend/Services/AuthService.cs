using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using PoliceAuthBackend.Data;
using PoliceAuthBackend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService
{
    private readonly DbContext _db;

    public AuthService(DbContext db)
    {
        _db = db;
    }

    public User? ValidateUser(string userID, string password)
    {
        using var conn = _db.GetConnection();
        conn.Open();

        string query = "SELECT * FROM user WHERE User_ID=@id";

        MySqlCommand cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", userID);

        using var reader = cmd.ExecuteReader();

        if (!reader.Read())
            return null;

        string storedHash = reader["Password"].ToString() ?? string.Empty;

        bool valid = BCrypt.Net.BCrypt.Verify(password, storedHash);
        if (!valid) return null;

        return new User
        {
            User_ID = reader["User_ID"].ToString()!,

            Name = reader["Name"].ToString() ?? string.Empty,
            Surname = reader["Surname"].ToString() ?? string.Empty,
            Email = reader["Email"].ToString() ?? string.Empty,
            Password = storedHash,

            Phone_Number = reader["Phone_Number"].ToString() ?? string.Empty,
            Address = reader["Address"].ToString() ?? string.Empty,
            Birth_Date = Convert.ToDateTime(reader["Birth_Date"]),
            ProfilePhoto = reader["ProfilePhoto"]?.ToString(),

            Role_ID = Convert.ToInt32(reader["Role_ID"]),
            Department_ID = Convert.ToInt32(reader["Department_ID"]),
            Status = reader["Status"].ToString() ?? "active"
        };
    }

    public User? GetUserById(string userId)
    {
        using var conn = _db.GetConnection();
        conn.Open();

        string query = "SELECT * FROM user WHERE User_ID=@uid";

        MySqlCommand cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@uid", userId);

        using var reader = cmd.ExecuteReader();

        if (!reader.Read())
            return null;

        return new User
        {
            User_ID = reader["User_ID"].ToString()!,

            Name = reader["Name"].ToString() ?? string.Empty,
            Surname = reader["Surname"].ToString() ?? string.Empty,
            Email = reader["Email"].ToString() ?? string.Empty,

            Phone_Number = reader["Phone_Number"].ToString() ?? string.Empty,
            Address = reader["Address"].ToString() ?? string.Empty,
            Birth_Date = Convert.ToDateTime(reader["Birth_Date"]),
            ProfilePhoto = reader["ProfilePhoto"]?.ToString(),

            Role_ID = Convert.ToInt32(reader["Role_ID"]),
            Department_ID = Convert.ToInt32(reader["Department_ID"]),
            Status = reader["Status"].ToString() ?? "active"
        };
    }

    public User? GetUserByEmailOrUsername(string input)
    {
        using var conn = _db.GetConnection();
        conn.Open();

        string query = @"SELECT * FROM user WHERE Email=@input OR Name=@input";

        MySqlCommand cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@input", input);

        using var reader = cmd.ExecuteReader();

        if (!reader.Read())
            return null;

        return new User
        {
            User_ID = reader["User_ID"].ToString()!,

            Name = reader["Name"].ToString() ?? string.Empty,
            Surname = reader["Surname"].ToString() ?? string.Empty,
            Email = reader["Email"].ToString() ?? string.Empty,

            Phone_Number = reader["Phone_Number"].ToString() ?? string.Empty,
            Address = reader["Address"].ToString() ?? string.Empty,
            Birth_Date = Convert.ToDateTime(reader["Birth_Date"]),
            ProfilePhoto = reader["ProfilePhoto"]?.ToString(),

            Role_ID = Convert.ToInt32(reader["Role_ID"]),
            Department_ID = Convert.ToInt32(reader["Department_ID"]),
            Status = reader["Status"].ToString() ?? "active"
        };
    }

    public bool RegisterUser(PoliceAuthBackend.Dtos.RegisterRequest req)
    {
        using var conn = _db.GetConnection();
        conn.Open();

        string checkQuery = @"SELECT COUNT(*) FROM user WHERE User_ID=@id OR Email=@email";

        MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@id", req.User_ID);
        checkCmd.Parameters.AddWithValue("@email", req.Email);

        int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (exists > 0) return false;

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.Password);

        string query = @"INSERT INTO user
        (User_ID, Name, Surname, Email, Password, Phone_Number, Address, Birth_Date, ProfilePhoto, Role_ID, Department_ID, Status)
        VALUES
        (@id, @name, @surname, @email, @password, @phone, @address, @birth, @photo, @role, @department, @status)";

        MySqlCommand cmd = new MySqlCommand(query, conn);

        cmd.Parameters.AddWithValue("@id", req.User_ID);
        cmd.Parameters.AddWithValue("@name", req.Name);
        cmd.Parameters.AddWithValue("@surname", req.Surname);
        cmd.Parameters.AddWithValue("@email", req.Email);
        cmd.Parameters.AddWithValue("@password", hashedPassword);
        cmd.Parameters.AddWithValue("@phone", req.Phone_Number);
        cmd.Parameters.AddWithValue("@address", req.Address);
        cmd.Parameters.AddWithValue("@birth", req.Birth_Date);
        cmd.Parameters.AddWithValue("@photo", req.ProfilePhoto);
        cmd.Parameters.AddWithValue("@role", 6);
        cmd.Parameters.AddWithValue("@department", 1);
        cmd.Parameters.AddWithValue("@status", "active");

        cmd.ExecuteNonQuery();
        return true;
    }

    public string GenerateOtp()
    {
        return new Random().Next(100000, 999999).ToString();
    }

    public void SaveOtp(string userId, string code) 
    {
        using var conn = _db.GetConnection();
        conn.Open();

        string query = @"INSERT INTO login_otp (User_ID, Code, Expiry)
                         VALUES (@uid, @code, @expiry)";

        MySqlCommand cmd = new MySqlCommand(query, conn);

        cmd.Parameters.AddWithValue("@uid", userId);
        cmd.Parameters.AddWithValue("@code", code);
        cmd.Parameters.AddWithValue("@expiry", DateTime.Now.AddMinutes(5));

        cmd.ExecuteNonQuery();
    }

    public bool VerifyOtp(string userId, string code) 
    {
        using var conn = _db.GetConnection();
        conn.Open();

        string query = @"SELECT * FROM login_otp
                         WHERE User_ID=@uid AND Code=@code
                         AND IsUsed=0 AND Expiry > NOW()";

        MySqlCommand cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@uid", userId);
        cmd.Parameters.AddWithValue("@code", code);

        using var reader = cmd.ExecuteReader();
        bool valid = reader.Read();

        reader.Close();

        if (valid)
        {
            string updateQuery = @"UPDATE login_otp
                                   SET IsUsed=1
                                   WHERE User_ID=@uid AND Code=@code";

            MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn);
            updateCmd.Parameters.AddWithValue("@uid", userId);
            updateCmd.Parameters.AddWithValue("@code", code);
            updateCmd.ExecuteNonQuery();
        }

        return valid;
    }

    public void UpdatePassword(string userId, string newPassword)
    {
        using var conn = _db.GetConnection();
        conn.Open();

        string hash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        string query = @"UPDATE user SET Password=@pass WHERE User_ID=@uid";

        MySqlCommand cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@pass", hash);
        cmd.Parameters.AddWithValue("@uid", userId);

        cmd.ExecuteNonQuery();
    }

    public string GenerateJwtToken(User user, IConfiguration config)
    {
        string roleName = GetRoleName(user.Role_ID);

        var claims = new[]
        {
            new Claim("userId", user.User_ID), 
            new Claim("departmentId", user.Department_ID.ToString()),
            new Claim(ClaimTypes.Role, roleName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Name)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
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