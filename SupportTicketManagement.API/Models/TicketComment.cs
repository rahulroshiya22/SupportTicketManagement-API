using SupportTicketManagement.API.Enums;

namespace SupportTicketManagement.API.Models
{
    public class TicketStatusLog
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public TicketStatus OldStatus { get; set; }
        public TicketStatus NewStatus { get; set; }
        public int ChangedById { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public Ticket Ticket { get; set; } = null!;
        public User ChangedBy { get; set; } = null!;
    }
}
