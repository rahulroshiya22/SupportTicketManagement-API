using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketManagement.API.DTOs.Comments;
using SupportTicketManagement.API.Enums;
using SupportTicketManagement.API.Services;

namespace SupportTicketManagement.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    [Tags("Comments")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private RoleName CurrentRole =>
            Enum.Parse<RoleName>(User.FindFirstValue(ClaimTypes.Role)!);

        // ── PATCH /comments/{id} (MANAGER or comment author) ───────────────────
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(CommentResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> EditComment(int id, [FromBody] CommentDTO dto)
        {
            try
            {
                var result = await _commentService.EditCommentAsync(id, dto.Comment, CurrentUserId, CurrentRole);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        }

        // ── DELETE /comments/{id} (MANAGER or comment author)
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                await _commentService.DeleteCommentAsync(id, CurrentUserId, CurrentRole);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return StatusCode(403, new { message = ex.Message }); }
        }
    }
}
