namespace SupportTicketManagement.API.DTOs.Auth
{
    public class AuthResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public UserInfoDTO User { get; set; } = null!;
    }

    public class UserInfoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public RoleDTO Role { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class RoleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
