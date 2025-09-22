using MyApp.DTOs;
using MyApp.DTOs.Auth;
using MyApp.Helpers;
using MyApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        // -----------------------------
        // Registration (no JWT)
        // -----------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return this.BadResponse("Invalid input data");

            var result = await _auth.RegisterAsync(dto);

            if (result == null)
                return this.OkResponse<object?>(null, "Registration successful. Please log in.");

            return this.OkResponse(result, "User created successfully");
        }

        // -----------------------------
        // Login (generate JWT + Refresh Token)
        // -----------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return this.BadResponse("Invalid input data");

            var loginResult = await _auth.LoginAsync(dto);

            if (loginResult == null)
                return this.BadResponse("Invalid credentials or user blocked", 401);

            return this.OkResponse(loginResult, "Login successful");
        }

        // -----------------------------
        // Refresh JWT using Refresh Token
        // -----------------------------
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return this.BadResponse("Refresh token is required");

            var result = await _auth.RefreshAsync(dto.RefreshToken);

            if (result == null)
                return this.BadResponse("Invalid or expired refresh token", 401);

            return this.OkResponse(result, "Token refreshed successfully");
        }

        // -----------------------------
        // Revoke Refresh Token (Logout)
        // -----------------------------
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeTokenDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return this.BadResponse("Refresh token is required");

            var success = await _auth.RevokeAsync(dto.RefreshToken);

            if (!success)
                return this.BadResponse("Invalid or already revoked token");

            return this.OkResponse<object?>(null, "Token revoked successfully");
        }
    }
}
