-- ============================================================
--  Support Ticket Management System — SQL Server Script
--  Database: SupportTicketManagementDB
- ============================================================



-- Create database 

CREATE DATABASE SupportTicketManagementDB;
   

USE SupportTicketManagementDB;
GO

-- ============================================================
--  DROP TABLES (reverse FK order, safe re-run)
-- ============================================================
IF OBJECT_ID('TicketStatusLogs','U') IS NOT NULL DROP TABLE TicketStatusLogs;
IF OBJECT_ID('TicketComments','U')   IS NOT NULL DROP TABLE TicketComments;
IF OBJECT_ID('Tickets','U')          IS NOT NULL DROP TABLE Tickets;
IF OBJECT_ID('Users','U')            IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('Roles','U')            IS NOT NULL DROP TABLE Roles;
IF OBJECT_ID('__EFMigrationsHistory','U') IS NOT NULL DROP TABLE __EFMigrationsHistory;
GO

-- ============================================================
--  TABLE: Roles
-- ============================================================
CREATE TABLE Roles (
    Id   INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(450) NOT NULL,

    CONSTRAINT PK_Roles PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX IX_Roles_Name ON Roles (Name);
GO

-- ============================================================
--  TABLE: Users
-- ============================================================
CREATE TABLE Users (
    Id        INT IDENTITY(1,1) NOT NULL,
    Name      NVARCHAR(255) NOT NULL,
    Email     NVARCHAR(255) NOT NULL,
    Password  NVARCHAR(255) NOT NULL,   -- BCrypt hash
    RoleId    INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT FK_Users_Roles_RoleId
        FOREIGN KEY (RoleId) REFERENCES Roles (Id)
        ON DELETE NO ACTION
);
CREATE UNIQUE INDEX IX_Users_Email ON Users (Email);
CREATE INDEX IX_Users_RoleId ON Users (RoleId);
GO

-- ============================================================
--  TABLE: Tickets
-- ============================================================
CREATE TABLE Tickets (
    Id           INT IDENTITY(1,1) NOT NULL,
    Title        NVARCHAR(255) NOT NULL,
    Description  NVARCHAR(MAX) NOT NULL,
    Status       NVARCHAR(MAX) NOT NULL DEFAULT N'OPEN',     -- OPEN | IN_PROGRESS | RESOLVED | CLOSED
    Priority     NVARCHAR(MAX) NOT NULL DEFAULT N'MEDIUM',   -- LOW | MEDIUM | HIGH
    CreatedById  INT NOT NULL,
    AssignedToId INT NULL,
    CreatedAt    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT PK_Tickets PRIMARY KEY (Id),
    CONSTRAINT FK_Tickets_Users_CreatedById
        FOREIGN KEY (CreatedById) REFERENCES Users (Id)
        ON DELETE NO ACTION,
    CONSTRAINT FK_Tickets_Users_AssignedToId
        FOREIGN KEY (AssignedToId) REFERENCES Users (Id)
        ON DELETE NO ACTION
);
CREATE INDEX IX_Tickets_CreatedById  ON Tickets (CreatedById);
CREATE INDEX IX_Tickets_AssignedToId ON Tickets (AssignedToId);
GO

-- ============================================================
--  TABLE: TicketComments
-- ============================================================
CREATE TABLE TicketComments (
    Id        INT IDENTITY(1,1) NOT NULL,
    TicketId  INT NOT NULL,
    UserId    INT NOT NULL,
    Comment   NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT PK_TicketComments PRIMARY KEY (Id),
    CONSTRAINT FK_TicketComments_Tickets_TicketId
        FOREIGN KEY (TicketId) REFERENCES Tickets (Id)
        ON DELETE CASCADE,
    CONSTRAINT FK_TicketComments_Users_UserId
        FOREIGN KEY (UserId) REFERENCES Users (Id)
        ON DELETE NO ACTION
);
CREATE INDEX IX_TicketComments_TicketId ON TicketComments (TicketId);
CREATE INDEX IX_TicketComments_UserId   ON TicketComments (UserId);
GO

-- ============================================================
--  TABLE: TicketStatusLogs  (audit trail)
-- ============================================================
CREATE TABLE TicketStatusLogs (
    Id          INT IDENTITY(1,1) NOT NULL,
    TicketId    INT NOT NULL,
    OldStatus   NVARCHAR(MAX) NOT NULL,
    NewStatus   NVARCHAR(MAX) NOT NULL,
    ChangedById INT NOT NULL,
    ChangedAt   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT PK_TicketStatusLogs PRIMARY KEY (Id),
    CONSTRAINT FK_TicketStatusLogs_Tickets_TicketId
        FOREIGN KEY (TicketId) REFERENCES Tickets (Id)
        ON DELETE CASCADE,
    CONSTRAINT FK_TicketStatusLogs_Users_ChangedById
        FOREIGN KEY (ChangedById) REFERENCES Users (Id)
        ON DELETE NO ACTION
);
CREATE INDEX IX_TicketStatusLogs_TicketId    ON TicketStatusLogs (TicketId);
CREATE INDEX IX_TicketStatusLogs_ChangedById ON TicketStatusLogs (ChangedById);
GO

-- ============================================================
--  EF Core Migrations History Table
-- ============================================================
CREATE TABLE __EFMigrationsHistory (
    MigrationId    NVARCHAR(150) NOT NULL,
    ProductVersion NVARCHAR(32) NOT NULL,
    CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
);
GO

-- ============================================================
--  SEED DATA
--  Note: Passwords below are BCrypt hashes.
--    admin@tms.com  → Admin@123
--    sarah/mike     → Support@123
--    alice/bob/carol→ User@123
-- ============================================================

-- Roles
INSERT INTO Roles (Name) VALUES ('MANAGER'), ('SUPPORT'), ('USER');
GO

-- Admin user (MANAGER, RoleId = 1)
INSERT INTO Users (Name, Email, Password, RoleId, CreatedAt) VALUES
('Admin Manager', 'admin@tms.com',
 '$2a$11$rQSpjKjjxrxH0XD3Kz3MaOmCR5K5pR.JEsVL1pASr8q8LJ.0K5TKO',
 1, GETUTCDATE());

-- Support users (RoleId = 2)
INSERT INTO Users (Name, Email, Password, RoleId, CreatedAt) VALUES
('Sarah Support', 'sarah@tms.com',
 '$2a$11$sampleBcryptHashForSarahSupportReplaceWithReal.Hash',
 2, GETUTCDATE()),
('Mike Tech', 'mike@tms.com',
 '$2a$11$sampleBcryptHashForMikeTechReplaceWithRealHashVal',
 2, GETUTCDATE());

-- Regular users (RoleId = 3)
INSERT INTO Users (Name, Email, Password, RoleId, CreatedAt) VALUES
('Alice Employee', 'alice@tms.com',
 '$2a$11$sampleBcryptHashForAliceReplaceWithRealHashValue',
 3, GETUTCDATE()),
('Bob Johnson', 'bob@tms.com',
 '$2a$11$sampleBcryptHashForBobJohnsonReplaceWithRealHash',
 3, GETUTCDATE()),
('Carol Smith', 'carol@tms.com',
 '$2a$11$sampleBcryptHashForCarolSmithReplaceWithRealHash',
 3, GETUTCDATE());
GO

PRINT '✅ SupportTicketManagementDB created and seeded successfully!';
PRINT '';
PRINT 'NOTE: The BCrypt password hashes in the seed data above are PLACEHOLDERS.';
PRINT 'For correct login, run the .NET API with dotnet run — it seeds real BCrypt hashes automatically.';
PRINT 'Or use this script only for schema creation and let the API seed the data.';
GO

