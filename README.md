# Customer Support Ticket System

A full-stack Customer Support Ticket System built as part of a technical assignment. The system consists of a C# WinForms desktop application as the frontend, an ASP.NET Web API as the backend, and MySQL as the database.

---

## About This Project

I built this system to demonstrate my understanding of C# desktop application development, REST API design, database design, and business logic implementation. The desktop application communicates exclusively through the Web API — there is no direct database access from the desktop app.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | C# WinForms (.NET 8, Windows) |
| Backend | ASP.NET Web API (.NET 8) |
| Database | MySQL 8.x |
| ORM / Data Access | Dapper (micro-ORM) |
| Authentication | JWT Bearer Tokens |
| Password Hashing | BCrypt.Net-Next |
| HTTP Communication | System.Net.Http (JSON over HTTP) |
| API Documentation | Swagger / OpenAPI |

---

## Project Structure

```
SupportTicketSystem/
├── SupportTicketSystem.sln
│
├── API/                              ← ASP.NET Web API
│   ├── Controllers/
│   │   ├── AuthController.cs         ← Login endpoint
│   │   └── TicketsController.cs      ← All ticket endpoints
│   ├── Data/
│   │   └── Repositories.cs           ← Dapper database access
│   ├── Models/
│   │   └── Models.cs                 ← Domain models and DTOs
│   ├── Services/
│   │   └── JwtService.cs             ← JWT token generation
│   ├── Program.cs                    ← App configuration and DI
│   └── appsettings.json              ← Connection string and JWT config
│
├── Desktop/                          ← WinForms Desktop App
│   ├── Forms/
│   │   ├── LoginForm.cs              ← Login screen
│   │   ├── MainForm.cs               ← Main shell with sidebar
│   │   ├── TicketListForm.cs         ← Ticket list with search
│   │   ├── CreateTicketForm.cs       ← Create new ticket
│   │   └── TicketDetailForm.cs       ← Ticket details + admin actions
│   ├── Models/
│   │   └── Models.cs                 ← Shared DTOs and session
│   ├── Services/
│   │   └── ApiClient.cs              ← HTTP calls to Web API
│   ├── UI/
│   │   └── Theme.cs                  ← Dark theme and custom controls
│   └── Program.cs                    ← Entry point
│
└── Database/
    └── schema.sql                    ← MySQL schema and seed data
```

---

## Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 8.0 or later |
| MySQL Server | 8.0 or later |
| Visual Studio | 2022 or later |
| MySQL Workbench | Optional (for viewing DB) |

---

## How to Run the Project Locally

### Step 1 — Set Up the Database

Open MySQL Workbench or the MySQL CLI and run the schema script:

```bash
mysql -u root -p < Database/schema.sql
```

Or in MySQL Workbench:
- File → Open SQL Script → select `Database/schema.sql`
- Click the ⚡ Execute button

This creates the `SupportTicketDB` database with all 4 tables and seed users.

Verify it worked:
```sql
USE SupportTicketDB;
SHOW TABLES;
SELECT Username, Role FROM Users;
```

---

### Step 2 — Configure the API

Open `API/appsettings.json` and update the connection string with your MySQL password:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=SupportTicketDB;Uid=root;Pwd=YOUR_PASSWORD;"
  },
  "JwtSettings": {
    "SecretKey": "SupportTicket_SuperSecretKey_2024_ChangeInProduction!",
    "Issuer": "SupportTicketAPI",
    "Audience": "SupportTicketSystem",
    "ExpiryHours": 8
  },
  "Urls": "http://localhost:5000"
}
```

---

### Step 3 — Run the API

```bash
cd API
dotnet restore
dotnet run
```

The API will start at `http://localhost:5000`. Open Swagger to verify:
```
http://localhost:5000/swagger
```

---

### Step 4 — Run the Desktop App

Open a new terminal (keep the API running):

```bash
cd Desktop
dotnet restore
dotnet run
```

Or open `SupportTicketSystem.sln` in Visual Studio 2022 and press **F5**.

---

### Step 5 — Run Both Together (Recommended)

In Visual Studio:
```
Right-click Solution → Properties
→ Multiple Startup Projects
→ SupportTicketAPI    → Start
→ SupportTicketDesktop → Start
→ OK → Press F5
```

---

## Login Credentials

| Username | Password | Role |
|---|---|---|
| `admin` | `admin123` | Admin |
| `jsmith` | `user123` | Admin |
| `alice` | `user123` | User |
| `bob` | `user123` | User |

---

## API Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/auth/hash/{password}` | None | Generate BCrypt hash for a password | This is optional only for checking password
| POST | `/api/auth/login` | None | Login and receive JWT token |
| GET | `/api/tickets` | Required | Get tickets (role-filtered) |
| POST | `/api/tickets` | User | Create new ticket |
| GET | `/api/tickets/{id}` | Required | Get ticket details with history |
| PUT | `/api/tickets/{id}/assign` | Admin | Assign ticket to admin |
| PUT | `/api/tickets/{id}/status` | Admin | Update ticket status |
| POST | `/api/tickets/{id}/comments` | Required | Add comment to ticket |
| GET | `/api/tickets/admins` | Admin | Get list of admin users |

---

## Database Design

| Table | Purpose |
|---|---|
| `Users` | Stores user accounts with roles (User / Admin) |
| `Tickets` | Stores all support tickets |
| `TicketStatusHistory` | Logs every status change and admin action |
| `TicketComments` | Stores public and internal comments |

---

## Business Rules Implemented

- Ticket numbers are auto-generated in the format `TKT-YYYYMM-XXXX`
- Status can only flow: **Open → In Progress → Closed**
- Users can only view and comment on their own tickets
- Admins can view, assign, update status, and add internal comments on all tickets
- Closed tickets cannot be modified or commented on
- All admin actions are recorded in the status history log
- All timestamps use server-side UTC time
- Passwords are hashed using BCrypt with cost factor 11
- JWT tokens expire after 8 hours

---

## Design Decisions

- **Dapper over Entity Framework** — I chose Dapper for full SQL control and a lightweight footprint, which made it easier to write optimized queries with JOINs for ticket details.
- **JWT Authentication** — Stateless token-based auth is well suited for desktop-to-API communication. Tokens carry the user's role so the API can enforce permissions without additional database lookups.
- **Custom WinForms UI** — Instead of using default WinForms controls, I built a custom dark theme with hand-painted controls (rounded buttons, badge labels, sidebar navigation) to produce a modern-looking interface.
- **Server-side timestamps** — All dates are stored in UTC using `UTC_TIMESTAMP()` in MySQL and converted to local time in the desktop app for display.
- **Internal comments** — Admins can post internal comments that are hidden from regular users, allowing internal discussion without exposing notes to customers.

---

## Assumptions

- The desktop application is intended for Windows only (WinForms requires Windows)
- A single MySQL instance running on localhost is used for development
- The API and desktop app run on the same machine during development
- BCrypt password hashing with cost factor 11 is sufficient for this use case