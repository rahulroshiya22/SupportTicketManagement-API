using Microsoft.EntityFrameworkCore;
using SupportTicketManagement.API.Data;
using SupportTicketManagement.API.DTOs.Users;
using SupportTicketManagement.API.Entities;
using SupportTicketManagement.API.Enums;

namespace SupportTicketManagement.API.Services
{
    public interface IUserService
    {
        Task<UserResponseDTO> CreateUserAsync(CreateUserDTO dto);
        Task<List<UserResponseDTO>> GetAllUsersAsync();
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db) => _db = db;

        public async Task<UserResponseDTO> CreateUserAsync(CreateUserDTO dto)
        {
            // Check email uniqueness
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email already in use.");

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role)
                ?? throw new InvalidOperationException("Role not found.");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = role.Id,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Reload with role
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

        private static UserResponseDTO MapToDTO(User u) => new()
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = new RoleInfoDTO { Id = u.Role.Id, Name = u.Role.Name.ToString() },
            CreatedAt = u.CreatedAt
        };
    }
}
