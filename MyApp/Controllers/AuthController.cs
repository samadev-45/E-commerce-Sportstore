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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.FailResponse("Invalid input data"));

            var loginResult = await _auth.LoginAsync(dto);

            if (loginResult == null)
                return Unauthorized(ApiResponse.FailResponse("Invalid credentials or user blocked"));

            // Cookies for production
            var accessCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddMinutes(30)
            };
            Response.Cookies.Append("accessToken", loginResult.Token, accessCookieOptions);

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", loginResult.RefreshToken, refreshCookieOptions);

            // Return JWT and refresh token in the response body for testing
            return Ok(ApiResponse.SuccessResponse(new
            {
                UserId = loginResult.UserId,
                Email = loginResult.Email,
                Role = loginResult.Role,
                Name = loginResult.Name,
                Token = loginResult.Token,            // JWT token
                RefreshToken = loginResult.RefreshToken // Refresh token
            }, "Login successful"));
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(ApiResponse.FailResponse("Refresh token missing"));

            var result = await _auth.RefreshAsync(refreshToken);
            if (result == null)
                return Unauthorized(ApiResponse.FailResponse("Invalid or expired refresh token"));

            // Set new cookies
            var accessCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddMinutes(30)
            };
            Response.Cookies.Append("accessToken", result.Token, accessCookieOptions);

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", result.RefreshToken, refreshCookieOptions);

            // Return new tokens in response body
            return Ok(ApiResponse.SuccessResponse(new
            {
                Token = result.Token,
                RefreshToken = result.RefreshToken
            }, "Token refreshed successfully"));
        }


        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(ApiResponse.FailResponse("Refresh token missing"));

            var success = await _auth.RevokeAsync(refreshToken);
            if (!success)
                return BadRequest(ApiResponse.FailResponse("Invalid or already revoked token"));

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            };

            Response.Cookies.Delete("accessToken", cookieOptions);
            Response.Cookies.Delete("refreshToken", cookieOptions);

            return Ok(ApiResponse.SuccessResponse(null, "Logged out successfully"));
        }
    }
}
