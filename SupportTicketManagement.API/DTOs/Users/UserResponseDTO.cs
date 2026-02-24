namespace SupportTicketManagement.API.DTOs.Users
{
    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public RoleInfoDTO Role { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class RoleInfoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
