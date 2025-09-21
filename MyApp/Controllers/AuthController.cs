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
                return this.OkResponse<object>(null, "Registration successful. Please log in.");

            return this.OkResponse(result, "User created successfully");
        }

        // -----------------------------
        // Login (generate JWT)
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
    }
}
