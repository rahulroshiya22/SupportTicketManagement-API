using Microsoft.EntityFrameworkCore;
using SupportTicketManagement.API.Data;
using SupportTicketManagement.API.DTOs.Users;
using SupportTicketManagement.API.Models;
using SupportTicketManagement.API.Enums;

namespace SupportTicketManagement.API.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<UserResponseDTO> CreateUserAsync(CreateUserDTO dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                throw new Exception("Email already in use.");

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role);
            if (role == null)
                throw new Exception("Role not found.");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = role.Id
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            user.Role = role;

            return MapToDTO(user);
        }

        public async Task<List<UserResponseDTO>> GetAllUsersAsync()
        {
            return await _db.Users
                .Include(u => u.Role)
                .Select(u => MapToDTO(u))
                .ToListAsync();
        }

        private static UserResponseDTO MapToDTO(User u)
        {
            return new UserResponseDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = new RoleInfoDTO { Id = u.Role.Id, Name = u.Role.Name.ToString() },
                CreatedAt = u.CreatedAt
            };
        }
    }
}
