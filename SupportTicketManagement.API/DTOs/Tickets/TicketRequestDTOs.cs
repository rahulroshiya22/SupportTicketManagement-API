using System.ComponentModel.DataAnnotations;
using SupportTicketManagement.API.Enums;

namespace SupportTicketManagement.API.DTOs.Tickets
{
    public class CreateTicketDTO
    {
        [Required]
        [MinLength(5, ErrorMessage = "Title must be at least 5 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
        public string Description { get; set; } = string.Empty;

        public TicketPriority Priority { get; set; } = TicketPriority.MEDIUM;
    }

    public class AssignDTO
    {
        [Required]
        public int UserId { get; set; }
    }

    public class UpdateStatusDTO
    {
        [Required]
        public TicketStatus Status { get; set; }
    }

    public class TicketFilterDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? Search { get; set; }
    }
}
