using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace PoliceFuncBackend.Data
{
    public class DbContext
    {
        private readonly IConfiguration _config;

        public DbContext(IConfiguration config)
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