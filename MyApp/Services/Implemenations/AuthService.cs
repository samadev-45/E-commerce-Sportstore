using AutoMapper;
using MyApp.Data;
using MyApp.DTOs.Auth;
using MyApp.Entities;
using MyApp.Helpers;
using MyApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;



namespace MyApp.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthService(AppDbContext context, IConfiguration config, IMapper mapper)
        {
            _context = context;
            _config = config;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // 1. Trim spaces and convert to lowercase
            var email = registerDto.Email.Trim().ToLowerInvariant();

            // 2. Validate email format manually (optional, regex in DTO usually handles this)
            var emailPattern = @"^[a-z0-9]+[a-z0-9._]*@[a-z0-9.-]+\.[a-z]{2,}$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern))
                throw new ArgumentException("Invalid email format.");

            // 3. Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
                return null;

            // 4. Create user
            var user = _mapper.Map<User>(registerDto);
            user.Email = email;
            user.PasswordHash = PasswordHelper.HashPassword(registerDto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 5. DO NOT generate JWT for registration
            return null; // just return null or custom message; controller can send success
        }


        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // 1. Trim spaces and convert email to lowercase
            var email = loginDto.Email.Trim().ToLowerInvariant();

            // 2. Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null) return null;

            // 3. Blocked users cannot login
            if (user.IsBlock) return null;

            // 4. Verify password
            if (!PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
                return null;

            // 5. Generate JWT token
            var token = JwtHelper.GenerateJwtToken(user, _config);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role,
                Name = user.Name
            };
        }

        public async Task<UserProfileDto?> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

    }
}
