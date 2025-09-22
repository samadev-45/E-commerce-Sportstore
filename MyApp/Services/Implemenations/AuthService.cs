using AutoMapper;
using MyApp.Data;
using MyApp.DTOs.Auth;
using MyApp.Entities;
using MyApp.Helpers;
using MyApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

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
            var email = registerDto.Email.Trim().ToLowerInvariant();

            var emailPattern = @"^[a-z0-9]+[a-z0-9._]*@[a-z0-9.-]+\.[a-z]{2,}$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern))
                throw new ArgumentException("Invalid email format.");

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
                return null;

            var user = _mapper.Map<User>(registerDto);
            user.Email = email;
            user.PasswordHash = PasswordHelper.HashPassword(registerDto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // No JWT issued at registration
            return null;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var email = loginDto.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null || user.IsBlock) return null;

            if (!PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
                return null;

            // Generate tokens
            var jwtToken = JwtHelper.GenerateJwtToken(user, _config);
            var refreshToken = GenerateRefreshToken();
            refreshToken.UserId = user.Id;

            // Save refresh token
            _context.Set<RefreshToken>().Add(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
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

        public async Task<RefreshTokenResponseDto?> RefreshAsync(string refreshToken)
        {
            var storedToken = await _context.Set<RefreshToken>()
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null || !storedToken.IsActive) return null;

            // Issue new JWT (reuse the same refresh token)
            var jwtToken = JwtHelper.GenerateJwtToken(storedToken.User, _config);

            return new RefreshTokenResponseDto
            {
                Token = jwtToken,
                RefreshToken = storedToken.Token
            };
        }

        public async Task<bool> RevokeAsync(string refreshToken)
        {
            var storedToken = await _context.Set<RefreshToken>()
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null || !storedToken.IsActive) return false;

            storedToken.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        private RefreshToken GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow
            };
        }
    }
}
