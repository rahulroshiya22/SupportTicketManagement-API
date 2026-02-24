using Microsoft.EntityFrameworkCore;
using SupportTicketManagement.API.Data;
using SupportTicketManagement.API.DTOs.Comments;
using SupportTicketManagement.API.DTOs.Users;
using SupportTicketManagement.API.Models;
using SupportTicketManagement.API.Enums;

namespace SupportTicketManagement.API.Services
{
    public class CommentService
    {
        private readonly AppDbContext _db;

        public CommentService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<CommentResponseDTO> AddCommentAsync(int ticketId, string text, int userId)
        {
            var ticket = await _db.Tickets.FindAsync(ticketId);
            if (ticket == null)
                throw new Exception("Ticket not found.");

            var comment = new TicketComment
            {
                TicketId = ticketId,
                UserId = userId,
                Comment = text
            };

            _db.TicketComments.Add(comment);
            await _db.SaveChangesAsync();

            return await GetCommentDTO(comment.Id);
        }

        public async Task<List<CommentResponseDTO>> GetCommentsAsync(int ticketId)
        {
            var ticket = await _db.Tickets.FindAsync(ticketId);
            if (ticket == null)
                throw new Exception("Ticket not found.");

            return await _db.TicketComments
                .Where(c => c.TicketId == ticketId)
                .Include(c => c.User).ThenInclude(u => u.Role)
                .OrderBy(c => c.CreatedAt)
                .Select(c => MapToDTO(c))
                .ToListAsync();
        }

        public async Task<CommentResponseDTO> EditCommentAsync(int commentId, string text, int userId, RoleName role)
        {
            var comment = await _db.TicketComments.FindAsync(commentId);
            if (comment == null)
                throw new Exception("Comment not found.");

            if (role != RoleName.MANAGER && comment.UserId != userId)
                throw new Exception("You can only edit your own comments.");

            comment.Comment = text;
            await _db.SaveChangesAsync();

            return await GetCommentDTO(commentId);
        }

        public async Task DeleteCommentAsync(int commentId, int userId, RoleName role)
        {
            var comment = await _db.TicketComments.FindAsync(commentId);
            if (comment == null)
                throw new Exception("Comment not found.");

            if (role != RoleName.MANAGER && comment.UserId != userId)
                throw new Exception("You can only delete your own comments.");

            _db.TicketComments.Remove(comment);
            await _db.SaveChangesAsync();
        }

        private async Task<CommentResponseDTO> GetCommentDTO(int commentId)
        {
            var c = await _db.TicketComments
                .Include(c => c.User).ThenInclude(u => u.Role)
                .FirstAsync(c => c.Id == commentId);

            return MapToDTO(c);
        }

        private static CommentResponseDTO MapToDTO(TicketComment c)
        {
            return new CommentResponseDTO
            {
                Id = c.Id,
                Comment = c.Comment,
                User = new UserResponseDTO
                {
                    Id = c.User.Id,
                    Name = c.User.Name,
                    Email = c.User.Email,
                    Role = new RoleInfoDTO { Id = c.User.Role.Id, Name = c.User.Role.Name.ToString() },
                    CreatedAt = c.User.CreatedAt
                },
                CreatedAt = c.CreatedAt
            };
        }
    }
}
