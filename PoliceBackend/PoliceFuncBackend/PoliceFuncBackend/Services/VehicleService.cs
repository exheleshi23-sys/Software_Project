using MySql.Data.MySqlClient;
using PoliceFuncBackend.Data;
using PoliceFuncBackend.Models;

namespace PoliceFuncBackend.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly PoliceDbContext _db;

        public VehicleService(PoliceDbContext db)
        {
            _db = db;
        }

        public async Task<Vehicle?> LookupVehicleAsync(string plate)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"
                SELECT 
                    Vehicle_ID,
                    PlateNumber,
                    Model,
                    Brand,
                    Color,
                    RegistrationNumber,
                    RegistratiionStatus,
                    USER_ID
                FROM vehicles
                WHERE PlateNumber = @plate
                LIMIT 1";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@plate", plate);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new Vehicle
            {
                Vehicle_ID = reader.GetInt32(reader.GetOrdinal("Vehicle_ID")),
                PlateNumber = reader.GetInt32(reader.GetOrdinal("PlateNumber")),
                Model = reader.GetString(reader.GetOrdinal("Model")),
                Brand = reader.GetString(reader.GetOrdinal("Brand")),
                Color = reader.GetString(reader.GetOrdinal("Color")),
                RegistrationNumber = reader.GetInt32(reader.GetOrdinal("RegistrationNumber")),
                RegistratiionStatus = reader.GetString(reader.GetOrdinal("RegistratiionStatus")),
                USER_ID = reader.IsDBNull(reader.GetOrdinal("USER_ID"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("USER_ID"))
            };
        }
    }
}