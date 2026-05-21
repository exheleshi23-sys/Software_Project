using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace PoliceFuncBackend.Data
{
    public class PoliceDbContext
    {
        private readonly IConfiguration _config;

        public PoliceDbContext(IConfiguration config)
        {
            _config = config;
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(
                _config.GetConnectionString("PoliceDb")
            );
        }
    }
}