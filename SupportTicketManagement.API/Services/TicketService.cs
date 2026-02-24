using Microsoft.EntityFrameworkCore;
using SupportTicketManagement.API.Data;
using SupportTicketManagement.API.DTOs.Tickets;
using SupportTicketManagement.API.DTOs.Users;
using SupportTicketManagement.API.Entities;
using SupportTicketManagement.API.Enums;

namespace SupportTicketManagement.API.Services
{
    public interface ITicketService
    {
        Task<TicketResponseDTO> CreateTicketAsync(CreateTicketDTO dto, int currentUserId);
        Task<List<TicketResponseDTO>> GetTicketsAsync(int currentUserId, RoleName role);
        Task<TicketResponseDTO> AssignTicketAsync(int ticketId, int targetUserId, int currentUserId);
        Task<TicketResponseDTO> UpdateStatusAsync(int ticketId, TicketStatus newStatus, int currentUserId);
        Task DeleteTicketAsync(int ticketId);
    }

    public class TicketService : ITicketService
    {
        private readonly AppDbContext _db;

        public TicketService(AppDbContext db) => _db = db;

        public async Task<TicketResponseDTO> CreateTicketAsync(CreateTicketDTO dto, int currentUserId)
        {
            var ticket = new Ticket
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                Status = TicketStatus.OPEN,
                CreatedById = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();

            return await GetTicketResponseAsync(ticket.Id);
        }

        public async Task<List<TicketResponseDTO>> GetTicketsAsync(int currentUserId, RoleName role)
        {
            IQueryable<Ticket> query = _db.Tickets
                .Include(t => t.CreatedBy).ThenInclude(u => u.Role)
                .Include(t => t.AssignedTo!).ThenInclude(u => u.Role);

            if (role == RoleName.SUPPORT)
                query = query.Where(t => t.AssignedToId == currentUserId);
            else if (role == RoleName.USER)
                query = query.Where(t => t.CreatedById == currentUserId);

            return await query.Select(t => MapToDTO(t)).ToListAsync();
        }

        public async Task<TicketResponseDTO> AssignTicketAsync(int ticketId, int targetUserId, int currentUserId)
        {
            var ticket = await _db.Tickets.FindAsync(ticketId)
                ?? throw new KeyNotFoundException("Ticket not found.");

            var targetUser = await _db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == targetUserId)
                ?? throw new KeyNotFoundException("User not found.");

            if (targetUser.Role.Name == RoleName.USER)
                throw new InvalidOperationException("Cannot assign ticket to a USER role. Only MANAGER or SUPPORT allowed.");

            ticket.AssignedToId = targetUserId;
            await _db.SaveChangesAsync();

            return await GetTicketResponseAsync(ticketId);
        }

        public async Task<TicketResponseDTO> UpdateStatusAsync(int ticketId, TicketStatus newStatus, int currentUserId)
        {
            var ticket = await _db.Tickets.FindAsync(ticketId)
                ?? throw new KeyNotFoundException("Ticket not found.");

            // Validate forward-only transition
            if ((int)newStatus != (int)ticket.Status + 1)
                throw new InvalidOperationException(
                    $"Invalid status transition: {ticket.Status} → {newStatus}. " +
                    $"Only forward steps allowed: OPEN→IN_PROGRESS→RESOLVED→CLOSED.");

            // Create audit log BEFORE changing status
            var log = new TicketStatusLog
            {
                TicketId = ticket.Id,
                OldStatus = ticket.Status,
                NewStatus = newStatus,
                ChangedById = currentUserId,
                ChangedAt = DateTime.UtcNow
            };

            ticket.Status = newStatus;
            _db.TicketStatusLogs.Add(log);
            await _db.SaveChangesAsync();

            return await GetTicketResponseAsync(ticketId);
        }

        public async Task DeleteTicketAsync(int ticketId)
        {
            var ticket = await _db.Tickets.FindAsync(ticketId)
                ?? throw new KeyNotFoundException("Ticket not found.");

            _db.Tickets.Remove(ticket);
            await _db.SaveChangesAsync();
        }

        // ── Helpers ─────────────────────────────────────────────────────────────
        private async Task<TicketResponseDTO> GetTicketResponseAsync(int ticketId)
        {
            var t = await _db.Tickets
                .Include(t => t.CreatedBy).ThenInclude(u => u.Role)
                .Include(t => t.AssignedTo!).ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(t => t.Id == ticketId)
                ?? throw new KeyNotFoundException("Ticket not found.");

            return MapToDTO(t);
        }

        private static UserResponseDTO MapUser(User u) => new()
        {
            Id = u.Id, Name = u.Name, Email = u.Email,
            Role = new RoleInfoDTO { Id = u.Role.Id, Name = u.Role.Name.ToString() },
            CreatedAt = u.CreatedAt
        };

        private static TicketResponseDTO MapToDTO(Ticket t) => new()
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status.ToString(),
            Priority = t.Priority.ToString(),
            CreatedBy = MapUser(t.CreatedBy),
            AssignedTo = t.AssignedTo == null ? null : MapUser(t.AssignedTo),
            CreatedAt = t.CreatedAt
        };
    }
}
