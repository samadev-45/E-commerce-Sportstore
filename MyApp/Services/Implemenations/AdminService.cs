using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyApp.Common;
using MyApp.Data;
using MyApp.DTOs.Admin;
using MyApp.Entities;
using MyApp.Services.Interfaces;

namespace MyApp.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AdminService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
    .Where(u => u.Role != "Admin")
    .ToListAsync();

            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, bool isBlocked)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsBlock = isBlocked;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<List<UserDto>> SearchUsersByNameAsync(string name)
        {
            var users = await _context.Users
                .Where(u => u.Name.Contains(name)) 
                .ToListAsync();

            return _mapper.Map<List<UserDto>>(users);
        }

    }
}


