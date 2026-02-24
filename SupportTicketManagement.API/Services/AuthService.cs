using Microsoft.EntityFrameworkCore;
using SupportTicketManagement.API.Data;
using SupportTicketManagement.API.DTOs.Auth;
using SupportTicketManagement.API.DTOs.Users;
using SupportTicketManagement.API.Helpers;

namespace SupportTicketManagement.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO?> LoginAsync(string email, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly JwtHelper _jwt;

        public AuthService(AppDbContext db, JwtHelper jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        public async Task<AuthResponseDTO?> LoginAsync(string email, string password)
        {
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
                return null;

            var token = _jwt.GenerateToken(user);

            return new AuthResponseDTO
            {
                Token = token,
                User = new UserInfoDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = new RoleDTO { Id = user.Role.Id, Name = user.Role.Name.ToString() },
                    CreatedAt = user.CreatedAt
                }
            };
        }
    }
}
