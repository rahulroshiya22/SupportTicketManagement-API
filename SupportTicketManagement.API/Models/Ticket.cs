using SupportTicketManagement.API.Enums;

namespace SupportTicketManagement.API.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketStatus Status { get; set; } = TicketStatus.OPEN;
        public TicketPriority Priority { get; set; } = TicketPriority.MEDIUM;
        public int CreatedById { get; set; }
        public int? AssignedToId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User CreatedBy { get; set; } = null!;
        public User? AssignedTo { get; set; }
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public ICollection<TicketStatusLog> StatusLogs { get; set; } = new List<TicketStatusLog>();
    }
}
