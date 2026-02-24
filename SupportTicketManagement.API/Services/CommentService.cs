using Microsoft.EntityFrameworkCore;
using SupportTicketManagement.API.Data;
using SupportTicketManagement.API.DTOs.Comments;
using SupportTicketManagement.API.DTOs.Users;
using SupportTicketManagement.API.Entities;
using SupportTicketManagement.API.Enums;

namespace SupportTicketManagement.API.Services
{
    public interface ICommentService
    {
        Task<CommentResponseDTO> AddCommentAsync(int ticketId, string text, int userId);
        Task<List<CommentResponseDTO>> GetCommentsAsync(int ticketId);
        Task<CommentResponseDTO> EditCommentAsync(int commentId, string text, int userId, RoleName role);
        Task DeleteCommentAsync(int commentId, int userId, RoleName role);
    }

    public class CommentService : ICommentService
    {
        private readonly AppDbContext _db;

        public CommentService(AppDbContext db) => _db = db;

        // ─── Access check: MANAGER always; SUPPORT if assigned; USER if creator
        private async Task EnsureCommentAccessAsync(int ticketId, int userId, RoleName role)
        {
            if (role == RoleName.MANAGER) return;

            var ticket = await _db.Tickets.FindAsync(ticketId)
                ?? throw new KeyNotFoundException("Ticket not found.");

            if (role == RoleName.SUPPORT && ticket.AssignedToId != userId)
                throw new UnauthorizedAccessException("SUPPORT can only comment on tickets assigned to them.");

            if (role == RoleName.USER && ticket.CreatedById != userId)
                throw new UnauthorizedAccessException("USER can only comment on their own tickets.");
        }

        public async Task<CommentResponseDTO> AddCommentAsync(int ticketId, string text, int userId)
        {
            // Ensure ticket exists
            _ = await _db.Tickets.FindAsync(ticketId)
                ?? throw new KeyNotFoundException("Ticket not found.");

            var comment = new TicketComment
            {
                TicketId = ticketId,
                UserId = userId,
                Comment = text,
                CreatedAt = DateTime.UtcNow
            };

            _db.TicketComments.Add(comment);
            await _db.SaveChangesAsync();

            return await GetCommentResponseAsync(comment.Id);
        }

        public async Task<List<CommentResponseDTO>> GetCommentsAsync(int ticketId)
        {
            _ = await _db.Tickets.FindAsync(ticketId)
                ?? throw new KeyNotFoundException("Ticket not found.");

            return await _db.TicketComments
                .Where(c => c.TicketId == ticketId)
                .Include(c => c.User).ThenInclude(u => u.Role)
                .OrderBy(c => c.CreatedAt)
                .Select(c => MapToDTO(c))
                .ToListAsync();
        }

        public async Task<CommentResponseDTO> EditCommentAsync(int commentId, string text, int userId, RoleName role)
        {
            var comment = await _db.TicketComments.FindAsync(commentId)
                ?? throw new KeyNotFoundException("Comment not found.");

            // Only author or MANAGER can edit
            if (role != RoleName.MANAGER && comment.UserId != userId)
                throw new UnauthorizedAccessException("You can only edit your own comments.");

            comment.Comment = text;
            await _db.SaveChangesAsync();

            return await GetCommentResponseAsync(commentId);
        }

        public async Task DeleteCommentAsync(int commentId, int userId, RoleName role)
        {
            var comment = await _db.TicketComments.FindAsync(commentId)
                ?? throw new KeyNotFoundException("Comment not found.");

            if (role != RoleName.MANAGER && comment.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own comments.");

            _db.TicketComments.Remove(comment);
            await _db.SaveChangesAsync();
        }

        // ── Helpers ─────────────────────────────────────────────────────────────
        private async Task<CommentResponseDTO> GetCommentResponseAsync(int commentId)
        {
            var c = await _db.TicketComments
                .Include(c => c.User).ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(c => c.Id == commentId)
                ?? throw new KeyNotFoundException("Comment not found.");
            return MapToDTO(c);
        }

        private static CommentResponseDTO MapToDTO(TicketComment c) => new()
        {
            Id = c.Id,
            Comment = c.Comment,
            User = new UserResponseDTO
            {
                Id = c.User.Id, Name = c.User.Name, Email = c.User.Email,
                Role = new RoleInfoDTO { Id = c.User.Role.Id, Name = c.User.Role.Name.ToString() },
                CreatedAt = c.User.CreatedAt
            },
            CreatedAt = c.CreatedAt
        };
    }
}
