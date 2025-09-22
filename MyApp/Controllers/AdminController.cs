using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using MyApp.DTOs.Admin;
using MyApp.Services.Interfaces;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // Get all users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(new { success = true, data = users });
        }

        // Get user by id
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { success = false, message = "User not found" });

            return Ok(new { success = true, data = user });
        }

        // Block / Unblock user
        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
        {
            var result = await _adminService.UpdateUserStatusAsync(id, dto.IsBlocked);
            if (!result)
                return NotFound(new { success = false, message = "User not found" });

            var status = dto.IsBlocked ? "blocked" : "unblocked";
            return Ok(new { success = true, message = $"User {status} successfully" });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string name)
        {
            var users = await _adminService.SearchUsersByNameAsync(name);
            return Ok(new ApiResponse<List<UserDto>>(users));
        }

    }
}
