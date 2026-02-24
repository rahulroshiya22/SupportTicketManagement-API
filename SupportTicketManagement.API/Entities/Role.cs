using SupportTicketManagement.API.Enums;

namespace SupportTicketManagement.API.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public RoleName Name { get; set; }

        // Navigation
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
