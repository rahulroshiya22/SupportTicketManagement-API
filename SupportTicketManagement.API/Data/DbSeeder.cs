using BCrypt.Net;
using SupportTicketManagement.API.Entities;
using SupportTicketManagement.API.Enums;

namespace SupportTicketManagement.API.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            // Seed Roles
            if (!db.Roles.Any())
            {
                db.Roles.AddRange(
                    new Role { Name = RoleName.MANAGER },
                    new Role { Name = RoleName.SUPPORT },
                    new Role { Name = RoleName.USER }
                );
                db.SaveChanges();
                Console.WriteLine("✅ Roles seeded: MANAGER, SUPPORT, USER");
            }

            // Seed initial MANAGER user
            if (!db.Users.Any(u => u.Email == "admin@tms.com"))
            {
                var managerRole = db.Roles.First(r => r.Name == RoleName.MANAGER);
                db.Users.Add(new User
                {
                    Name = "Admin Manager",
                    Email = "admin@tms.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    RoleId = managerRole.Id,
                    CreatedAt = DateTime.UtcNow
                });
                db.SaveChanges();
                Console.WriteLine("✅ Admin user seeded: admin@tms.com / Admin@123");
            }
        }
    }
}
