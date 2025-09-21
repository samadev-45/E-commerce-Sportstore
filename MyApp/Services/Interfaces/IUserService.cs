using MyApp.DTOs.Auth;


namespace MyApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
    }
}