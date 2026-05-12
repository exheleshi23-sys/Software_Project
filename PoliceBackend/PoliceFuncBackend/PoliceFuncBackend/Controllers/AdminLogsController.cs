using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.Services;

namespace PoliceFuncBackend.Controllers
{
    [ApiController]
    [Route("api/admin/logs")]
    public class AdminLogsController : ControllerBase
    {
        private readonly AdminLogService _logs;

        public AdminLogsController(AdminLogService logs)
        {
            _logs = logs;
        }

        private bool IsAdmin() => true;

        [HttpGet]
        public IActionResult GetLogs()
        {
            if (!IsAdmin()) return Unauthorized();
            return Ok(_logs.GetLogs());
        }
    }
}