using SupportTicketManagement.API.Enums;

namespace SupportTicketManagement.API.Models
{
    public class Role
    {
        public int Id { get; set; }
        public RoleName Name { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
