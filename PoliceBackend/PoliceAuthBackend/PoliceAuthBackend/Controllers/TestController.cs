using Microsoft.AspNetCore.Mvc;
using PoliceAuthBackend.Data;
using Microsoft.AspNetCore.Authorization;

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

        [HttpGet("debug-auth")]
        public IActionResult DebugAuth()
        {
            var header = Request.Headers["Authorization"].ToString();

            return Ok(new
            {
                authorizationHeader = header
            });
        }

        [Authorize]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("OK");
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