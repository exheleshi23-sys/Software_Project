using Microsoft.AspNetCore.Mvc;
using PoliceAuthBackend.Data;

namespace PoliceAuthBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly DbContext _dbContext;

        public TestController(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("db")]
        public IActionResult TestDb()
        {
            try
            {
                using var conn = _dbContext.GetConnection();
                conn.Open();

                return Ok("Database connected successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}