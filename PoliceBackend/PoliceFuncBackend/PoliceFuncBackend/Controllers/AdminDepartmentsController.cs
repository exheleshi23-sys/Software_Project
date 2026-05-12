using Microsoft.AspNetCore.Mvc;
using PoliceFuncBackend.Services;
using PoliceFuncBackend.DTOs;

namespace PoliceFuncBackend.Controllers
{
    [ApiController]
    [Route("api/admin/departments")]
    public class AdminDepartmentsController : ControllerBase
    {
        private readonly DepartmentService _departmentService;

        public AdminDepartmentsController(DepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // TEMP AUTH (replace with JWT later)
        private bool IsAdmin()
        {
            return true;
        }

        // GET ALL DEPARTMENTS
        [HttpGet]
        public IActionResult GetAll()
        {
            if (!IsAdmin())
                return Unauthorized();

            var result = _departmentService.GetDepartments();
            return Ok(result);
        }

        // CREATE DEPARTMENT
        [HttpPost]
        public IActionResult Create(DepartmentDto dto)
        {
            if (!IsAdmin())
                return Unauthorized();

            _departmentService.CreateDepartment(dto);
            return Ok("Department created");
        }

        // UPDATE DEPARTMENT
        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateDepartmentDto dto)
        {
            if (!IsAdmin())
                return Unauthorized();

            _departmentService.UpdateDepartment(id, dto);
            return Ok("Department updated");
        }

        // DELETE DEPARTMENT
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!IsAdmin())
                return Unauthorized();

            _departmentService.DeleteDepartment(id);
            return Ok("Department deleted");
        }
    }
}