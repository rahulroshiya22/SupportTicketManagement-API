using System.ComponentModel.DataAnnotations;
using SupportTicketManagement.API.DTOs.Users;

namespace SupportTicketManagement.API.DTOs.Comments
{
    public class CommentDTO
    {
        [Required(ErrorMessage = "Comment text is required")]
        public string Comment { get; set; } = string.Empty;
    }

    public class CommentResponseDTO
    {
        public int Id { get; set; }
        public string Comment { get; set; } = string.Empty;
        public UserResponseDTO User { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
