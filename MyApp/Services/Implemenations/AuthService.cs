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
            // Normalize email check
            var email = registerDto.Email.Trim().ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
                return null;

            var user = _mapper.Map<User>(registerDto);
            user.Email = email;
            user.PasswordHash = PasswordHelper.HashPassword(registerDto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

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

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var email = loginDto.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user == null) return null;
            if (user.IsBlock) return null; // blocked users cannot login

            if (!PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
                return null;

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
