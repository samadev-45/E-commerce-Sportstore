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

        //  Get all users
        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse>> GetAllUsers()
        {
            var users = await _adminService.GetAllUsersAsync();

            if (users == null || !users.Any())
                return NotFound(ApiResponse.FailResponse("No users found", 404));

            return Ok(ApiResponse.SuccessResponse(users, "Users retrieved successfully"));
        }

        //  Get user by id
        [HttpGet("users/{id}")]
        public async Task<ActionResult<ApiResponse>> GetUserById(int id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponse.FailResponse("User not found", 404));

            return Ok(ApiResponse.SuccessResponse(user, "User retrieved successfully"));
        }

        //  Toggle Block/Unblock user
        [HttpPut("users/{id}/toggle-status")]
        public async Task<ActionResult<ApiResponse>> ToggleUserStatus(int id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponse.FailResponse("User not found", 404));

            // Flip the status automatically
            var newStatus = !user.IsBlocked;
            var result = await _adminService.UpdateUserStatusAsync(id, newStatus);

            if (!result)
                return StatusCode(500, ApiResponse.FailResponse("Failed to update user status", 500));

            var statusText = newStatus ? "blocked" : "unblocked";
            return Ok(ApiResponse.SuccessResponse(null, $"User {statusText} successfully"));
        }

        //  Search users by name
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse>> SearchUsers([FromQuery] string name)
        {
            var users = await _adminService.SearchUsersByNameAsync(name);

            if (users == null || !users.Any())
                return NotFound(ApiResponse.FailResponse("No users found with given name", 404));

            return Ok(ApiResponse.SuccessResponse(users, "Users retrieved successfully"));
        }
    }
}
