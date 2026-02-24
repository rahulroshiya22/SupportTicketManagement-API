using Microsoft.EntityFrameworkCore;
using SupportTicketManagement.API.Data;
using SupportTicketManagement.API.DTOs.Tickets;
using SupportTicketManagement.API.DTOs.Users;
using SupportTicketManagement.API.Models;
using SupportTicketManagement.API.Enums;

namespace SupportTicketManagement.API.Services
{
    public class TicketService
    {
        private readonly AppDbContext _db;

        public TicketService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<TicketResponseDTO> CreateTicketAsync(CreateTicketDTO dto, int userId)
        {
            var ticket = new Ticket
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                Status = TicketStatus.OPEN,
                CreatedById = userId
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();

            return await GetTicketDTO(ticket.Id);
        }

        public async Task<PagedResultDTO<TicketResponseDTO>> GetTicketsAsync(int userId, RoleName role, TicketFilterDTO filter)
        {
            var query = _db.Tickets
                .Include(t => t.CreatedBy).ThenInclude(u => u.Role)
                .Include(t => t.AssignedTo!).ThenInclude(u => u.Role)
                .AsQueryable();

            if (role == RoleName.SUPPORT)
                query = query.Where(t => t.AssignedToId == userId);
            else if (role == RoleName.USER)
                query = query.Where(t => t.CreatedById == userId);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(t => t.Status == Enum.Parse<TicketStatus>(filter.Status, true));

            if (!string.IsNullOrEmpty(filter.Priority))
                query = query.Where(t => t.Priority == Enum.Parse<TicketPriority>(filter.Priority, true));

            if (!string.IsNullOrEmpty(filter.Search))
                query = query.Where(t => t.Title.Contains(filter.Search) || t.Description.Contains(filter.Search));

            var total = await query.CountAsync();
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

            var data = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => MapToDTO(t))
                .ToListAsync();

            return new PagedResultDTO<TicketResponseDTO>
            {
                Data = data,
                Total = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)total / pageSize)
            };
        }

        public async Task<TicketResponseDTO> AssignTicketAsync(int ticketId, int targetUserId)
        {
            var ticket = await _db.Tickets.FindAsync(ticketId);
            if (ticket == null)
                throw new Exception("Ticket not found.");

            var targetUser = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == targetUserId);
            if (targetUser == null)
                throw new Exception("User not found.");

            if (targetUser.Role.Name == RoleName.USER)
                throw new Exception("Cannot assign ticket to a USER role.");

            ticket.AssignedToId = targetUserId;
            await _db.SaveChangesAsync();

            return await GetTicketDTO(ticketId);
        }

        public async Task<TicketResponseDTO> UpdateStatusAsync(int ticketId, TicketStatus newStatus, int userId)
        {
            var ticket = await _db.Tickets.FindAsync(ticketId);
            if (ticket == null)
                throw new Exception("Ticket not found.");

            if ((int)newStatus != (int)ticket.Status + 1)
                throw new Exception($"Invalid status transition: {ticket.Status} -> {newStatus}");

            var log = new TicketStatusLog
            {
                TicketId = ticket.Id,
                OldStatus = ticket.Status,
                NewStatus = newStatus,
                ChangedById = userId
            };

            ticket.Status = newStatus;
            _db.TicketStatusLogs.Add(log);
            await _db.SaveChangesAsync();

            return await GetTicketDTO(ticketId);
        }

        public async Task DeleteTicketAsync(int ticketId)
        {
            var ticket = await _db.Tickets.FindAsync(ticketId);
            if (ticket == null)
                throw new Exception("Ticket not found.");

            _db.Tickets.Remove(ticket);
            await _db.SaveChangesAsync();
        }

        private async Task<TicketResponseDTO> GetTicketDTO(int ticketId)
        {
            var t = await _db.Tickets
                .Include(t => t.CreatedBy).ThenInclude(u => u.Role)
                .Include(t => t.AssignedTo!).ThenInclude(u => u.Role)
                .FirstAsync(t => t.Id == ticketId);

            return MapToDTO(t);
        }

        private static UserResponseDTO MapUser(User u)
        {
            return new UserResponseDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = new RoleInfoDTO { Id = u.Role.Id, Name = u.Role.Name.ToString() },
                CreatedAt = u.CreatedAt
            };
        }

        private static TicketResponseDTO MapToDTO(Ticket t)
        {
            return new TicketResponseDTO
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
}
