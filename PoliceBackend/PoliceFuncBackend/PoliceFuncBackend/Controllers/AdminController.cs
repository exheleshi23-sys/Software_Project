using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.DTOs;
using PoliceFuncBackend.Services;

namespace PoliceFuncBackend.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    public class AdminController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;

        public AdminController(UserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        // TEMP ROLE CHECK (replace later with JWT)
        private bool IsAdmin()
        {
            return true; // placeholder for now
        }

        // -----------------------------
        // GET ALL USERS
        // -----------------------------
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            if (!IsAdmin())
                return Unauthorized();

            var users = _userService.GetUsers();
            return Ok(users);
        }

        // -----------------------------
        // GET USER BY ID
        // -----------------------------
        [HttpGet("{id}")]
        public IActionResult GetUserById(String id)
        {
            if (!IsAdmin())
                return Unauthorized();

            var user = _userService.GetUserById(id);

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }

        [Authorize]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("You are authenticated");
        }

        // -----------------------------
        // CREATE USER
        // -----------------------------
        [HttpPost]
        public IActionResult CreateUser(UserDto dto)
        {
            if (!IsAdmin())
                return Unauthorized();

            _userService.CreateUser(dto);
            return Ok("User created successfully");
        }

        // -----------------------------
        // UPDATE USER INFO
        // -----------------------------
        [HttpPut("{id}")]
        public IActionResult UpdateUser(String id, UpdateUserDto dto)
        {
            if (!IsAdmin())
                return Unauthorized();

            _userService.UpdateUser(id, dto);
            return Ok("User updated successfully");
        }

        // -----------------------------
        // DELETE USER (SOFT DELETE)
        // -----------------------------
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(String id)
        {
            if (!IsAdmin())
                return Unauthorized();

            _userService.UpdateStatus(id, "suspended");
            return Ok("User suspended (soft delete)");
        }

        // -----------------------------
        // UPDATE STATUS
        // /api/admin/users/:id/status
        // -----------------------------
        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(String id, UpdateStatusDto dto)
        {
            if (!IsAdmin())
                return Unauthorized();

            if (dto.Status != "active" && dto.Status != "suspended")
                return BadRequest("Invalid status value");

            _userService.UpdateStatus(id, dto.Status);
            return Ok("Status updated successfully");
        }

        // -----------------------------
        // UPDATE ROLE
        // /api/admin/users/:id/role
        // -----------------------------
        [HttpPut("{id}/role")]
        public IActionResult UpdateRole(String id, UpdateRoleDto dto)
        {
            if (!IsAdmin())
                return Unauthorized();

            _userService.UpdateRole(id, dto.Role_ID);
            return Ok("Role updated successfully");
        }
    }
}