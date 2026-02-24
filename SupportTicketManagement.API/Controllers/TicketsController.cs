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
    public class TicketsController : ControllerBase
    {
        private readonly TicketService _ticketService;

        public TicketsController(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private RoleName CurrentRole => Enum.Parse<RoleName>(User.FindFirstValue(ClaimTypes.Role)!);

        [HttpPost]
        [Authorize(Roles = "MANAGER,USER")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ticket = await _ticketService.CreateTicketAsync(dto, CurrentUserId);
            return StatusCode(201, ticket);
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets([FromQuery] TicketFilterDTO filter)
        {
            var tickets = await _ticketService.GetTicketsAsync(CurrentUserId, CurrentRole, filter);
            return Ok(tickets);
        }

        [HttpPatch("{id}/assign")]
        [Authorize(Roles = "MANAGER,SUPPORT")]
        public async Task<IActionResult> AssignTicket(int id, [FromBody] AssignDTO dto)
        {
            try
            {
                var ticket = await _ticketService.AssignTicketAsync(id, dto.UserId);
                return Ok(ticket);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "MANAGER,SUPPORT")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDTO dto)
        {
            try
            {
                var ticket = await _ticketService.UpdateStatusAsync(id, dto.Status, CurrentUserId);
                return Ok(ticket);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "MANAGER")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            try
            {
                await _ticketService.DeleteTicketAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
