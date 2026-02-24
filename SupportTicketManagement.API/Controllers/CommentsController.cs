using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketManagement.API.Data;
using SupportTicketManagement.API.DTOs.Comments;
using SupportTicketManagement.API.Enums;
using SupportTicketManagement.API.Services;

namespace SupportTicketManagement.API.Controllers
{
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;
        private readonly AppDbContext _db;

        public CommentsController(CommentService commentService, AppDbContext db)
        {
            _commentService = commentService;
            _db = db;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private RoleName CurrentRole => Enum.Parse<RoleName>(User.FindFirstValue(ClaimTypes.Role)!);

        [HttpPost("Tickets/{ticketId}/comments")]
        public async Task<IActionResult> AddComment(int ticketId, [FromBody] CommentDTO dto)
        {
            var ticket = _db.Tickets.Find(ticketId);
            if (ticket == null)
                return NotFound(new { message = "Ticket not found." });

            var role = CurrentRole;
            var userId = CurrentUserId;

            if (role == RoleName.SUPPORT && ticket.AssignedToId != userId)
                return Forbid();
            if (role == RoleName.USER && ticket.CreatedById != userId)
                return Forbid();

            try
            {
                var comment = await _commentService.AddCommentAsync(ticketId, dto.Comment, userId);
                return StatusCode(201, comment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("Tickets/{ticketId}/comments")]
        public async Task<IActionResult> GetComments(int ticketId)
        {
            var ticket = _db.Tickets.Find(ticketId);
            if (ticket == null)
                return NotFound(new { message = "Ticket not found." });

            var role = CurrentRole;
            var userId = CurrentUserId;

            if (role == RoleName.SUPPORT && ticket.AssignedToId != userId)
                return Forbid();
            if (role == RoleName.USER && ticket.CreatedById != userId)
                return Forbid();

            var comments = await _commentService.GetCommentsAsync(ticketId);
            return Ok(comments);
        }

        [HttpPatch("Comments/{id}")]
        public async Task<IActionResult> EditComment(int id, [FromBody] CommentDTO dto)
        {
            try
            {
                var comment = await _commentService.EditCommentAsync(id, dto.Comment, CurrentUserId, CurrentRole);
                return Ok(comment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("Comments/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                await _commentService.DeleteCommentAsync(id, CurrentUserId, CurrentRole);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
