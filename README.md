# Support Ticket Management System — .NET 8 Web API

A backend API for a company helpdesk system built with **ASP.NET Core .NET 8**, **Entity Framework Core**, and **SQL Server**.

---

## Tech Stack

- ASP.NET Core .NET 8
- Entity Framework Core 8 (SQL Server)
- JWT Authentication
- BCrypt Password Hashing
- Swagger UI

---

## Project Structure

```
SupportTicketManagement.API/
├── Controllers/        → API endpoints
├── Models/             → Database models (User, Role, Ticket, etc.)
├── DTOs/               → Request and response shapes
├── Services/           → Business logic
├── Data/               → DbContext and Seeder
├── Enums/              → RoleName, TicketStatus, TicketPriority
├── Helpers/            → JwtHelper
├── Migrations/         → EF Core migrations
├── Program.cs          → App configuration
└── appsettings.json    → Connection string and JWT config
```

---

## Setup & Run

### 1. Clone the repo
```bash
git clone https://github.com/YOUR_USERNAME/support-ticket-management.git
cd "support-ticket-management"
```

### 2. Update connection string
Edit `SupportTicketManagement.API/appsettings.json`:
```json
"DefaultConnection": "Server=YOUR_SERVER_NAME;Database=SupportTicketManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

### 3. Run migrations
```bash
cd SupportTicketManagement.API
dotnet ef database update
```

### 4. Run the API
```bash
dotnet run
```

### 5. Open Swagger
```
http://localhost:5280/docs
```

The app will auto-seed 3 roles and one admin user on first run.

---

## Default Login Credentials

| Role | Email | Password |
|---|---|---|
| MANAGER | admin@tms.com | Admin@123 |
| SUPPORT | sarah@tms.com | Support@123 |
| SUPPORT | mike@tms.com | Support@123 |
| USER | alice@tms.com | User@123 |
| USER | bob@tms.com | User@123 |
| USER | carol@tms.com | User@123 |

---

## Database Tables

| Table | Description |
|---|---|
| Roles | MANAGER, SUPPORT, USER |
| Users | User accounts with BCrypt hashed passwords |
| Tickets | Support tickets |
| TicketComments | Comments on tickets |
| TicketStatusLogs | Audit log for every status change |

---

## API Endpoints

### Auth
| Method | Route | Access | Description |
|---|---|---|---|
| POST | `/Auth/login` | Public | Login and get JWT token |

### Users
| Method | Route | Access | Description |
|---|---|---|---|
| POST | `/Users` | MANAGER | Create a new user |
| GET | `/Users` | MANAGER | Get all users |

### Tickets
| Method | Route | Access | Description |
|---|---|---|---|
| POST | `/Tickets` | MANAGER, USER | Create a ticket |
| GET | `/Tickets` | All (role-filtered) | Get tickets with pagination and filters |
| PATCH | `/Tickets/{id}/assign` | MANAGER, SUPPORT | Assign ticket to a user |
| PATCH | `/Tickets/{id}/status` | MANAGER, SUPPORT | Update ticket status |
| DELETE | `/Tickets/{id}` | MANAGER | Delete a ticket |

#### GET /Tickets — Query Parameters
| Param | Example | Description |
|---|---|---|
| page | `1` | Page number |
| pageSize | `10` | Items per page |
| status | `OPEN` | Filter by status |
| priority | `HIGH` | Filter by priority |
| search | `vpn` | Search in title and description |

**Example:** `GET /Tickets?page=1&pageSize=5&status=OPEN&priority=HIGH`

#### Response format
```json
{
  "data": [ ...tickets... ],
  "total": 25,
  "page": 1,
  "pageSize": 5,
  "totalPages": 5
}
```

### Comments
| Method | Route | Access | Description |
|---|---|---|---|
| POST | `/Tickets/{ticketId}/comments` | All (access-checked) | Add a comment |
| GET | `/Tickets/{ticketId}/comments` | All (access-checked) | Get comments |
| PATCH | `/Comments/{id}` | Author or MANAGER | Edit a comment |
| DELETE | `/Comments/{id}` | Author or MANAGER | Delete a comment |

---

## Roles & Permissions

| Action | MANAGER | SUPPORT | USER |
|---|---|---|---|
| Login | ✅ | ✅ | ✅ |
| Create user | ✅ | ❌ | ❌ |
| View all tickets | ✅ | ❌ | ❌ |
| View assigned tickets | — | ✅ | ❌ |
| View own tickets | — | ❌ | ✅ |
| Create ticket | ✅ | ❌ | ✅ |
| Assign ticket | ✅ | ✅ | ❌ |
| Update status | ✅ | ✅ | ❌ |
| Delete ticket | ✅ | ❌ | ❌ |
| Comment on any ticket | ✅ | ❌ | ❌ |
| Comment on assigned ticket | — | ✅ | ❌ |
| Comment on own ticket | — | ❌ | ✅ |

---

## Ticket Status Flow

```
OPEN → IN_PROGRESS → RESOLVED → CLOSED
```

- Only forward transitions are allowed
- Every status change is recorded in `TicketStatusLogs` with who changed it and when

---

## Input Validation

- Title: minimum 5 characters
- Description: minimum 10 characters
- Priority: must be `LOW`, `MEDIUM`, or `HIGH`
- Status: must be `OPEN`, `IN_PROGRESS`, `RESOLVED`, or `CLOSED`

---

## Dashboard

Open `TMSDoc/TMS_Dashboard.html` in your browser while the API is running.

Features:
- Login with role quick-fill buttons
- Role-based stats and charts
- Ticket list with filters
- Ticket detail popup with comments
- User management (MANAGER only)

---

## Database Setup (Alternative to Migrations)

Run `scripts/SupportTicketManagement_DB.sql` in SQL Server Management Studio.

---

## How to Use Swagger

1. Open `http://localhost:5280/docs`
2. Login via `POST /Auth/login` — copy the `token` from response
3. Click **Authorize** button at the top
4. Enter: `Bearer YOUR_TOKEN_HERE`
5. Now all protected endpoints are unlocked
