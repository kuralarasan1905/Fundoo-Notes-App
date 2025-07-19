-- =============================================
-- Fundoo Notes Database Creation Script
-- =============================================

-- Create Database (if it doesn't exist)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'FundooNotesDB')
BEGIN
    CREATE DATABASE FundooNotesDB;
END
GO

USE FundooNotesDB;
GO

-- =============================================
-- Drop existing tables (in correct order due to foreign keys)
-- =============================================
IF OBJECT_ID('dbo.Collaborators', 'U') IS NOT NULL DROP TABLE dbo.Collaborators;
IF OBJECT_ID('dbo.NoteLabels', 'U') IS NOT NULL DROP TABLE dbo.NoteLabels;
IF OBJECT_ID('dbo.Notes', 'U') IS NOT NULL DROP TABLE dbo.Notes;
IF OBJECT_ID('dbo.Labels', 'U') IS NOT NULL DROP TABLE dbo.Labels;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- =============================================
-- Create Users Table
-- =============================================
CREATE TABLE dbo.Users (
    Id int IDENTITY(1,1) NOT NULL,
    FirstName nvarchar(100) NOT NULL,
    LastName nvarchar(100) NOT NULL,
    Email nvarchar(255) NOT NULL,
    PasswordHash nvarchar(max) NOT NULL,
    IsEmailVerified bit NOT NULL DEFAULT 0,
    EmailVerificationToken nvarchar(500) NULL,
    EmailVerificationTokenExpiry datetime2(7) NULL,
    PasswordResetToken nvarchar(500) NULL,
    PasswordResetTokenExpiry datetime2(7) NULL,
    LastLoginAt datetime2(7) NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted bit NOT NULL DEFAULT 0,
    DeletedAt datetime2(7) NULL,
    CONSTRAINT PK_Users PRIMARY KEY (Id)
);
GO

-- Create indexes for Users table
CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users (Email) WHERE IsDeleted = 0;
CREATE INDEX IX_Users_EmailVerificationToken ON dbo.Users (EmailVerificationToken);
CREATE INDEX IX_Users_PasswordResetToken ON dbo.Users (PasswordResetToken);
GO

-- =============================================
-- Create Labels Table
-- =============================================
CREATE TABLE dbo.Labels (
    Id int IDENTITY(1,1) NOT NULL,
    Name nvarchar(50) NOT NULL,
    Color nvarchar(7) NULL,
    UserId int NOT NULL,
    CreatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted bit NOT NULL DEFAULT 0,
    DeletedAt datetime2(7) NULL,
    CONSTRAINT PK_Labels PRIMARY KEY (Id),
    CONSTRAINT FK_Labels_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
);
GO

-- Create indexes for Labels table
CREATE INDEX IX_Labels_UserId ON dbo.Labels (UserId);
CREATE UNIQUE INDEX IX_Labels_UserId_Name ON dbo.Labels (UserId, Name) WHERE IsDeleted = 0;
GO

-- =============================================
-- Create Notes Table
-- =============================================
CREATE TABLE dbo.Notes (
    Id int IDENTITY(1,1) NOT NULL,
    Title nvarchar(200) NOT NULL,
    Content nvarchar(max) NULL,
    Color nvarchar(7) NULL,
    IsPinned bit NOT NULL DEFAULT 0,
    IsArchived bit NOT NULL DEFAULT 0,
    IsTrashed bit NOT NULL DEFAULT 0,
    ReminderDateTime datetime2(7) NULL,
    UserId int NOT NULL,
    CreatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted bit NOT NULL DEFAULT 0,
    DeletedAt datetime2(7) NULL,
    CONSTRAINT PK_Notes PRIMARY KEY (Id),
    CONSTRAINT FK_Notes_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE CASCADE
);
GO

-- Create indexes for Notes table
CREATE INDEX IX_Notes_UserId ON dbo.Notes (UserId);
CREATE INDEX IX_Notes_IsPinned ON dbo.Notes (IsPinned);
CREATE INDEX IX_Notes_IsArchived ON dbo.Notes (IsArchived);
CREATE INDEX IX_Notes_IsTrashed ON dbo.Notes (IsTrashed);
CREATE INDEX IX_Notes_ReminderDateTime ON dbo.Notes (ReminderDateTime);
CREATE INDEX IX_Notes_UserId_CreatedAt ON dbo.Notes (UserId, CreatedAt);
GO

-- =============================================
-- Create NoteLabels Table (Junction Table)
-- =============================================
CREATE TABLE dbo.NoteLabels (
    Id int IDENTITY(1,1) NOT NULL,
    NoteId int NOT NULL,
    LabelId int NOT NULL,
    CreatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted bit NOT NULL DEFAULT 0,
    DeletedAt datetime2(7) NULL,
    CONSTRAINT PK_NoteLabels PRIMARY KEY (Id),
    CONSTRAINT FK_NoteLabels_Notes_NoteId FOREIGN KEY (NoteId) REFERENCES dbo.Notes (Id) ON DELETE CASCADE,
    CONSTRAINT FK_NoteLabels_Labels_LabelId FOREIGN KEY (LabelId) REFERENCES dbo.Labels (Id) ON DELETE NO ACTION
);
GO

-- Create indexes for NoteLabels table
CREATE INDEX IX_NoteLabels_NoteId ON dbo.NoteLabels (NoteId);
CREATE INDEX IX_NoteLabels_LabelId ON dbo.NoteLabels (LabelId);
CREATE UNIQUE INDEX IX_NoteLabels_NoteId_LabelId ON dbo.NoteLabels (NoteId, LabelId) WHERE IsDeleted = 0;
GO

-- =============================================
-- Create Collaborators Table
-- =============================================
CREATE TABLE dbo.Collaborators (
    Id int IDENTITY(1,1) NOT NULL,
    NoteId int NOT NULL,
    UserId int NOT NULL,
    Permission nvarchar(50) NOT NULL DEFAULT 'Read',
    IsAccepted bit NOT NULL DEFAULT 0,
    AcceptedAt datetime2(7) NULL,
    CreatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted bit NOT NULL DEFAULT 0,
    DeletedAt datetime2(7) NULL,
    CONSTRAINT PK_Collaborators PRIMARY KEY (Id),
    CONSTRAINT FK_Collaborators_Notes_NoteId FOREIGN KEY (NoteId) REFERENCES dbo.Notes (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Collaborators_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE NO ACTION
);
GO

-- Create indexes for Collaborators table
CREATE INDEX IX_Collaborators_NoteId ON dbo.Collaborators (NoteId);
CREATE INDEX IX_Collaborators_UserId ON dbo.Collaborators (UserId);
CREATE UNIQUE INDEX IX_Collaborators_NoteId_UserId ON dbo.Collaborators (NoteId, UserId) WHERE IsDeleted = 0;
GO

-- =============================================
-- Insert Sample Data (Optional)
-- =============================================

-- Insert a test user
INSERT INTO dbo.Users (FirstName, LastName, Email, PasswordHash, IsEmailVerified, IsActive)
VALUES ('Test', 'User', 'test@fundoonotes.com', '$2a$11$example.hash.for.testing.purposes', 1, 1);

-- Get the user ID
DECLARE @UserId int = SCOPE_IDENTITY();

-- Insert sample labels
INSERT INTO dbo.Labels (Name, Color, UserId)
VALUES
    ('Personal', '#FF5722', @UserId),
    ('Work', '#2196F3', @UserId),
    ('Important', '#F44336', @UserId),
    ('Ideas', '#9C27B0', @UserId);

-- Insert sample notes
INSERT INTO dbo.Notes (Title, Content, Color, UserId)
VALUES
    ('Welcome to Fundoo Notes', 'This is your first note! You can create, edit, and organize your notes here.', '#FFF9C4', @UserId),
    ('Meeting Notes', 'Discuss project timeline and deliverables for Q1 2025.', '#E1F5FE', @UserId),
    ('Shopping List', 'Milk, Bread, Eggs, Fruits, Vegetables', '#F3E5F5', @UserId);

-- Link notes with labels
DECLARE @PersonalLabelId int = (SELECT Id FROM dbo.Labels WHERE Name = 'Personal' AND UserId = @UserId);
DECLARE @WorkLabelId int = (SELECT Id FROM dbo.Labels WHERE Name = 'Work' AND UserId = @UserId);
DECLARE @WelcomeNoteId int = (SELECT Id FROM dbo.Notes WHERE Title = 'Welcome to Fundoo Notes' AND UserId = @UserId);
DECLARE @MeetingNoteId int = (SELECT Id FROM dbo.Notes WHERE Title = 'Meeting Notes' AND UserId = @UserId);
DECLARE @ShoppingNoteId int = (SELECT Id FROM dbo.Notes WHERE Title = 'Shopping List' AND UserId = @UserId);

INSERT INTO dbo.NoteLabels (NoteId, LabelId)
VALUES
    (@MeetingNoteId, @WorkLabelId),
    (@ShoppingNoteId, @PersonalLabelId);

GO

-- =============================================
-- Create Views for Easy Data Access
-- =============================================

-- View for Notes with Labels
CREATE VIEW vw_NotesWithLabels AS
SELECT
    n.Id as NoteId,
    n.Title,
    n.Content,
    n.Color,
    n.IsPinned,
    n.IsArchived,
    n.IsTrashed,
    n.ReminderDateTime,
    n.UserId,
    n.CreatedAt,
    n.UpdatedAt,
    u.FirstName + ' ' + u.LastName as UserName,
    u.Email as UserEmail,
    STRING_AGG(l.Name, ', ') as Labels
FROM dbo.Notes n
INNER JOIN dbo.Users u ON n.UserId = u.Id
LEFT JOIN dbo.NoteLabels nl ON n.Id = nl.NoteId AND nl.IsDeleted = 0
LEFT JOIN dbo.Labels l ON nl.LabelId = l.Id AND l.IsDeleted = 0
WHERE n.IsDeleted = 0 AND u.IsDeleted = 0
GROUP BY n.Id, n.Title, n.Content, n.Color, n.IsPinned, n.IsArchived, n.IsTrashed,
         n.ReminderDateTime, n.UserId, n.CreatedAt, n.UpdatedAt, u.FirstName, u.LastName, u.Email;
GO

-- View for User Statistics
CREATE VIEW vw_UserStatistics AS
SELECT
    u.Id as UserId,
    u.FirstName + ' ' + u.LastName as UserName,
    u.Email,
    COUNT(n.Id) as TotalNotes,
    SUM(CASE WHEN n.IsPinned = 1 THEN 1 ELSE 0 END) as PinnedNotes,
    SUM(CASE WHEN n.IsArchived = 1 THEN 1 ELSE 0 END) as ArchivedNotes,
    SUM(CASE WHEN n.IsTrashed = 1 THEN 1 ELSE 0 END) as TrashedNotes,
    COUNT(DISTINCT l.Id) as TotalLabels,
    u.LastLoginAt,
    u.CreatedAt as UserCreatedAt
FROM dbo.Users u
LEFT JOIN dbo.Notes n ON u.Id = n.UserId AND n.IsDeleted = 0
LEFT JOIN dbo.Labels l ON u.Id = l.UserId AND l.IsDeleted = 0
WHERE u.IsDeleted = 0
GROUP BY u.Id, u.FirstName, u.LastName, u.Email, u.LastLoginAt, u.CreatedAt;
GO

PRINT 'Fundoo Notes Database created successfully!';
PRINT 'Tables created: Users, Notes, Labels, NoteLabels, Collaborators';
PRINT 'Views created: vw_NotesWithLabels, vw_UserStatistics';
PRINT 'Sample data inserted for testing.';
GO
