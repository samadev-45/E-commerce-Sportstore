using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using MyApp.DTOs;
using MyApp.DTOs.Auth;
using MyApp.Helpers;
using MyApp.Services.Interfaces;

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
                return BadRequest(ApiResponse.FailResponse("Invalid input data"));

            var result = await _auth.RegisterAsync(dto);

            if (result == null)
                return Ok(ApiResponse.SuccessResponse(null, "Registration successful. Please log in."));

            return Ok(ApiResponse.SuccessResponse(result, "User created successfully"));
        }

        // -----------------------------
        // Login (generate JWT + Refresh Token)
        // -----------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.FailResponse("Invalid input data"));

            var loginResult = await _auth.LoginAsync(dto);

            if (loginResult == null)
                return Unauthorized(ApiResponse.FailResponse("Invalid credentials or user blocked"));

            return Ok(ApiResponse.SuccessResponse(loginResult, "Login successful"));
        }

        // -----------------------------
        // Refresh JWT using Refresh Token
        // -----------------------------
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return BadRequest(ApiResponse.FailResponse("Refresh token is required"));

            var result = await _auth.RefreshAsync(dto.RefreshToken);

            if (result == null)
                return Unauthorized(ApiResponse.FailResponse("Invalid or expired refresh token"));

            return Ok(ApiResponse.SuccessResponse(result, "Token refreshed successfully"));
        }

        // -----------------------------
        // Revoke Refresh Token (Logout)
        // -----------------------------
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeTokenDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return BadRequest(ApiResponse.FailResponse("Refresh token is required"));

            var success = await _auth.RevokeAsync(dto.RefreshToken);

            if (!success)
                return BadRequest(ApiResponse.FailResponse("Invalid or already revoked token"));

            return Ok(ApiResponse.SuccessResponse(null, "Token revoked successfully"));
        }
    }
}
