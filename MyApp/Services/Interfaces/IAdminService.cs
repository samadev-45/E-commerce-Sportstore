using MyApp.DTOs.Admin;

namespace MyApp.Services.Interfaces
{
    public interface IAdminService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserStatusAsync(int userId, bool isBlocked);
        Task<List<UserDto>> SearchUsersByNameAsync(string name);
    }
}
