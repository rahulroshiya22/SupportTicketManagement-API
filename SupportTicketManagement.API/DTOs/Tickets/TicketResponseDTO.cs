using SupportTicketManagement.API.DTOs.Users;

namespace SupportTicketManagement.API.DTOs.Tickets
{
    public class TicketResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public UserResponseDTO CreatedBy { get; set; } = null!;
        public UserResponseDTO? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
