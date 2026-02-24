# 🎫 Support Ticket Management System — .NET 8 Web API

A production-ready **REST API** for a company helpdesk system built with **ASP.NET Core .NET 8**, **Entity Framework Core**, and **SQL Server**. Supports a full ticket lifecycle with role-based access control (RBAC), JWT authentication, and audit logging.

---

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Database Setup](#-database-setup)
- [Configuration](#-configuration)
- [API Endpoints](#-api-endpoints)
- [Roles & Permissions](#-roles--permissions)
- [Dashboard](#-dashboard)
- [Default Credentials](#-default-credentials)

---

## ✨ Features

- 🔐 **JWT Bearer Authentication** — stateless token-based auth
- 👥 **3-Role RBAC** — `MANAGER`, `SUPPORT`, `USER`
- 🎫 **Full Ticket Lifecycle** — `OPEN → IN_PROGRESS → RESOLVED → CLOSED`
- 📝 **Comments** — role-aware, author-only edit/delete
- 📊 **Status Audit Log** — every status change recorded with actor + timestamp
- 🔒 **BCrypt Password Hashing**
- 📖 **Swagger UI** with Bearer token support at `/docs`
- 🌐 **HTML Dashboard** — single-page role-based dashboard included

---

## 🛠 Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core **8.0** |
| ORM | Entity Framework Core **8.0.12** |
| Database | Microsoft **SQL Server** |
| Auth | JWT Bearer — `Microsoft.AspNetCore.Authentication.JwtBearer 8.0.12` |
| Password | `BCrypt.Net-Next 4.0.3` |
| Docs | `Swashbuckle.AspNetCore 6.9.0` |
| Language | C# 12 / .NET 8 |

---

## 📁 Project Structure

```
d:\api backhad exam\
├── SupportTicketManagement.API\
│   ├── Controllers\
│   │   ├── AuthController.cs
│   │   ├── UsersController.cs
│   │   ├── TicketsController.cs
│   │   └── CommentsController.cs
│   ├── Data\
│   │   ├── AppDbContext.cs
│   │   └── DbSeeder.cs
│   ├── DTOs\
│   │   ├── Auth\        (LoginDTO, AuthResponseDTO)
│   │   ├── Users\       (CreateUserDTO, UserResponseDTO)
│   │   ├── Tickets\     (TicketRequestDTOs, TicketResponseDTO)
│   │   └── Comments\    (CommentDTOs)
│   ├── Entities\
│   │   ├── Role.cs
│   │   ├── User.cs
│   │   ├── Ticket.cs
│   │   ├── TicketComment.cs
│   │   └── TicketStatusLog.cs
│   ├── Enums\
│   │   ├── RoleName.cs
│   │   ├── TicketStatus.cs
│   │   └── TicketPriority.cs
│   ├── Helpers\
│   │   └── JwtHelper.cs
│   ├── Migrations\
│   ├── Services\
│   │   ├── AuthService.cs
│   │   ├── UserService.cs
│   │   ├── TicketService.cs
│   │   └── CommentService.cs
│   ├── appsettings.json
│   └── Program.cs
├── TMSDoc\
│   ├── TMS_Dashboard.html      ← Single-page dashboard UI
│   └── TMS_API_Deploy_View.html
├── scripts\
│   └── SupportTicketManagement_DB.sql  ← Full DB script
└── README.md
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft SQL Server (any edition)
- Git

### 1. Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/support-ticket-management.git
cd "support-ticket-management"
```

### 2. Configure Connection String

Edit `SupportTicketManagement.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SQL_SERVER_NAME;Database=SupportTicketManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

> Replace `YOUR_SQL_SERVER_NAME` with your SQL Server instance name (e.g. `RAHUL`, `localhost`, `.\SQLEXPRESS`)

### 3. Apply Database Migration

```bash
cd SupportTicketManagement.API
dotnet ef database update
```

> Or use the SQL script directly: `scripts/SupportTicketManagement_DB.sql`

### 4. Run the API

```bash
dotnet run
```

### 5. Open Swagger UI

```
http://localhost:5280/docs
```

---

## 🗄️ Database Setup

### Option A — EF Core Migration *(Recommended)*
```bash
dotnet ef database update
```
Database `SupportTicketManagementDB` and all tables are created automatically. The app seeds roles and an admin user on first run.

### Option B — SQL Script
Run `scripts/SupportTicketManagement_DB.sql` in SQL Server Management Studio (SSMS) or Azure Data Studio.

### Tables Created

| Table | Description |
|---|---|
| `Roles` | MANAGER, SUPPORT, USER |
| `Users` | User accounts with BCrypt passwords |
| `Tickets` | Support tickets with status & priority |
| `TicketComments` | Comments on tickets |
| `TicketStatusLogs` | Audit log for every status change |

---

## ⚙️ Configuration

`appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=RAHUL;Database=SupportTicketManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "TMS_SuperSecret_Key_2026_AtLeast32Chars!",
    "Issuer": "SupportTicketManagementAPI",
    "Audience": "SupportTicketManagementClients",
    "ExpiryHours": 8
  }
}
```

> ⚠️ Change the `Jwt:Key` before deploying to production!

---

## 📡 API Endpoints

### Auth
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/Auth/login` | Public | Get JWT token |

### Users
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/Users` | MANAGER | Create a user |
| `GET` | `/Users` | MANAGER | List all users |

### Tickets
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/Tickets` | MANAGER, USER | Create ticket |
| `GET` | `/Tickets` | All (role-filtered) | List tickets |
| `PATCH` | `/Tickets/{id}/assign` | MANAGER, SUPPORT | Assign to user |
| `PATCH` | `/Tickets/{id}/status` | MANAGER, SUPPORT | Update status |
| `DELETE` | `/Tickets/{id}` | MANAGER | Delete ticket |
| `POST` | `/Tickets/{id}/comments` | All (access-checked) | Add comment |
| `GET` | `/Tickets/{id}/comments` | All (access-checked) | Get comments |

### Comments
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `PATCH` | `/Comments/{id}` | Author or MANAGER | Edit comment |
| `DELETE` | `/Comments/{id}` | Author or MANAGER | Delete comment |

---

## 🛡️ Roles & Permissions

| Action | MANAGER | SUPPORT | USER |
|---|---|---|---|
| Login | ✅ | ✅ | ✅ |
| Create users | ✅ | ❌ | ❌ |
| View all tickets | ✅ | ❌ | ❌ |
| View assigned tickets | ✅ | ✅ | ❌ |
| View own tickets | ✅ | ❌ | ✅ |
| Create ticket | ✅ | ❌ | ✅ |
| Assign ticket | ✅ | ✅ | ❌ |
| Update status | ✅ | ✅ | ❌ |
| Delete ticket | ✅ | ❌ | ❌ |
| Comment (any ticket) | ✅ | ❌ | ❌ |
| Comment (assigned) | ✅ | ✅ | ❌ |
| Comment (own ticket) | ✅ | ❌ | ✅ |

### Ticket Status Flow
```
OPEN → IN_PROGRESS → RESOLVED → CLOSED
```
Forward-only transitions. Every change is logged in `TicketStatusLogs`.

---

## 🌐 Dashboard

A single HTML file dashboard is included at `TMSDoc/TMS_Dashboard.html`.

**Features:**
- Login with quick-fill buttons for each role
- Role-based dashboard (Manager sees all, Support sees assigned, User sees own)
- Stats cards + bar charts
- Ticket list with filters (Status / Priority / Search / Assigned)
- Click any ticket → full detail modal with comments
- User management table (Manager only)

> Open `TMSDoc/TMS_Dashboard.html` in any browser while the API is running.

---

## 🔑 Default Credentials

| Role | Email | Password |
|---|---|---|
| 👑 MANAGER | `admin@tms.com` | `Admin@123` |
| 🛠 SUPPORT | `sarah@tms.com` | `Support@123` |
| 🛠 SUPPORT | `mike@tms.com` | `Support@123` |
| 👤 USER | `alice@tms.com` | `User@123` |
| 👤 USER | `bob@tms.com` | `User@123` |
| 👤 USER | `carol@tms.com` | `User@123` |

> These are seeded automatically on first run via `DbSeeder.cs`

---

## 📄 License

MIT — free to use for learning and projects.

---

**Built with ❤️ using .NET 8 | Entity Framework Core | SQL Server | JWT**
