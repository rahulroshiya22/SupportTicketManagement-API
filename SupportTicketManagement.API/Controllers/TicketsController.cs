using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketManagement.API.DTOs.Tickets;
using SupportTicketManagement.API.Enums;
using SupportTicketManagement.API.Services;

namespace SupportTicketManagement.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    [Tags("Tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ICommentService _commentService;

        public TicketsController(ITicketService ticketService, ICommentService commentService)
        {
            _ticketService = ticketService;
            _commentService = commentService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private RoleName CurrentRole =>
            Enum.Parse<RoleName>(User.FindFirstValue(ClaimTypes.Role)!);

        // ── POST /tickets (USER, MANAGER) ──────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "MANAGER,USER")]
        [ProducesResponseType(typeof(TicketResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDTO dto)
        {
            var ticket = await _ticketService.CreateTicketAsync(dto, CurrentUserId);
            return StatusCode(201, ticket);
        }

        // ── GET /tickets (role-filtered) ────────────────────────────────────────
        [HttpGet]
        [ProducesResponseType(typeof(List<TicketResponseDTO>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetTickets()
        {
            var tickets = await _ticketService.GetTicketsAsync(CurrentUserId, CurrentRole);
            return Ok(tickets);
        }

        // ── PATCH /tickets/{id}/assign (MANAGER, SUPPORT) ──────────────────────
        [HttpPatch("{id}/assign")]
        [Authorize(Roles = "MANAGER,SUPPORT")]
        [ProducesResponseType(typeof(TicketResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AssignTicket(int id, [FromBody] AssignDTO dto)
        {
            try
            {
                var ticket = await _ticketService.AssignTicketAsync(id, dto.UserId, CurrentUserId);
                return Ok(ticket);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ── PATCH /tickets/{id}/status (MANAGER, SUPPORT) ──────────────────────
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "MANAGER,SUPPORT")]
        [ProducesResponseType(typeof(TicketResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDTO dto)
        {
            try
            {
                var ticket = await _ticketService.UpdateStatusAsync(id, dto.Status, CurrentUserId);
                return Ok(ticket);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ── DELETE /tickets/{id} (MANAGER only) ────────────────────────────────
        [HttpDelete("{id}")]
        [Authorize(Roles = "MANAGER")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            try
            {
                await _ticketService.DeleteTicketAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // ── POST /tickets/{id}/comments ─────────────────────────────────────────
        [HttpPost("{id}/comments")]
        [ProducesResponseType(typeof(SupportTicketManagement.API.DTOs.Comments.CommentResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddComment(int id, [FromBody] SupportTicketManagement.API.DTOs.Comments.CommentDTO dto)
        {
            try
            {
                // Access check via service
                var roleForCheck = CurrentRole;
                // Inline check: MANAGER always, SUPPORT if assigned, USER if owner
                if (roleForCheck != RoleName.MANAGER)
                {
                    var ticket = await GetTicketForAccessCheck(id);
                    if (ticket == null)
                        return NotFound(new { message = "Ticket not found." });

                    if (roleForCheck == RoleName.SUPPORT && ticket.AssignedToId != CurrentUserId)
                        return Forbid();
                    if (roleForCheck == RoleName.USER && ticket.CreatedById != CurrentUserId)
                        return Forbid();
                }

                var comment = await _commentService.AddCommentAsync(id, dto.Comment, CurrentUserId);
                return StatusCode(201, comment);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        // ── GET /tickets/{id}/comments ──────────────────────────────────────────
        [HttpGet("{id}/comments")]
        [ProducesResponseType(typeof(List<SupportTicketManagement.API.DTOs.Comments.CommentResponseDTO>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetComments(int id)
        {
            try
            {
                var roleForCheck = CurrentRole;
                if (roleForCheck != RoleName.MANAGER)
                {
                    var ticket = await GetTicketForAccessCheck(id);
                    if (ticket == null)
                        return NotFound(new { message = "Ticket not found." });
                    if (roleForCheck == RoleName.SUPPORT && ticket.AssignedToId != CurrentUserId)
                        return Forbid();
                    if (roleForCheck == RoleName.USER && ticket.CreatedById != CurrentUserId)
                        return Forbid();
                }

                var comments = await _commentService.GetCommentsAsync(id);
                return Ok(comments);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // Helper to get minimal ticket info for access checks
        private async Task<SupportTicketManagement.API.Entities.Ticket?> GetTicketForAccessCheck(int ticketId)
        {
            return await Task.FromResult(
                HttpContext.RequestServices
                    .GetRequiredService<SupportTicketManagement.API.Data.AppDbContext>()
                    .Tickets.Find(ticketId));
        }
    }
}
