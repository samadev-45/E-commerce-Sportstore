using MyApp.DTOs.Auth;

namespace MyApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<UserProfileDto?> GetProfileAsync(int userId);
        Task<RefreshTokenResponseDto?> RefreshAsync(string refreshToken);
        Task<bool> RevokeAsync(string refreshToken);

    }
}
