-- =============================================
-- Enhanced Google Keep Features Migration Script
-- =============================================

USE FundooNotesDB;
GO

-- =============================================
-- Create NoteAttachments Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NoteAttachments')
BEGIN
    CREATE TABLE dbo.NoteAttachments (
        Id int IDENTITY(1,1) NOT NULL,
        NoteId int NOT NULL,
        FileName nvarchar(255) NOT NULL,
        FileType nvarchar(100) NOT NULL,
        FileUrl nvarchar(500) NOT NULL,
        FileSize bigint NOT NULL,
        FileHash nvarchar(32) NULL,
        CreatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted bit NOT NULL DEFAULT 0,
        DeletedAt datetime2(7) NULL,
        CONSTRAINT PK_NoteAttachments PRIMARY KEY (Id),
        CONSTRAINT FK_NoteAttachments_Notes_NoteId FOREIGN KEY (NoteId) REFERENCES dbo.Notes (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_NoteAttachments_NoteId ON dbo.NoteAttachments (NoteId);
    CREATE INDEX IX_NoteAttachments_FileHash ON dbo.NoteAttachments (FileHash);
    
    PRINT 'NoteAttachments table created successfully.';
END
ELSE
BEGIN
    PRINT 'NoteAttachments table already exists.';
END
GO

-- =============================================
-- Create NoteReminders Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NoteReminders')
BEGIN
    CREATE TABLE dbo.NoteReminders (
        Id int IDENTITY(1,1) NOT NULL,
        NoteId int NOT NULL,
        ReminderDateTime datetime2(7) NOT NULL,
        ReminderType nvarchar(20) NOT NULL DEFAULT 'Once',
        IsCompleted bit NOT NULL DEFAULT 0,
        CompletedAt datetime2(7) NULL,
        LastTriggeredAt datetime2(7) NULL,
        NextTriggerAt datetime2(7) NULL,
        Notes nvarchar(500) NULL,
        CreatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted bit NOT NULL DEFAULT 0,
        DeletedAt datetime2(7) NULL,
        CONSTRAINT PK_NoteReminders PRIMARY KEY (Id),
        CONSTRAINT FK_NoteReminders_Notes_NoteId FOREIGN KEY (NoteId) REFERENCES dbo.Notes (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_NoteReminders_NoteId ON dbo.NoteReminders (NoteId);
    CREATE INDEX IX_NoteReminders_ReminderDateTime ON dbo.NoteReminders (ReminderDateTime);
    CREATE INDEX IX_NoteReminders_IsCompleted_ReminderDateTime ON dbo.NoteReminders (IsCompleted, ReminderDateTime);
    
    PRINT 'NoteReminders table created successfully.';
END
ELSE
BEGIN
    PRINT 'NoteReminders table already exists.';
END
GO

-- =============================================
-- Create NoteListItems Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NoteListItems')
BEGIN
    CREATE TABLE dbo.NoteListItems (
        Id int IDENTITY(1,1) NOT NULL,
        NoteId int NOT NULL,
        Text nvarchar(1000) NOT NULL,
        IsCompleted bit NOT NULL DEFAULT 0,
        [Order] int NOT NULL DEFAULT 0,
        CompletedAt datetime2(7) NULL,
        CreatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted bit NOT NULL DEFAULT 0,
        DeletedAt datetime2(7) NULL,
        CONSTRAINT PK_NoteListItems PRIMARY KEY (Id),
        CONSTRAINT FK_NoteListItems_Notes_NoteId FOREIGN KEY (NoteId) REFERENCES dbo.Notes (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_NoteListItems_NoteId ON dbo.NoteListItems (NoteId);
    CREATE INDEX IX_NoteListItems_NoteId_Order ON dbo.NoteListItems (NoteId, [Order]);
    
    PRINT 'NoteListItems table created successfully.';
END
ELSE
BEGIN
    PRINT 'NoteListItems table already exists.';
END
GO

-- =============================================
-- Create NoteTemplates Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NoteTemplates')
BEGIN
    CREATE TABLE dbo.NoteTemplates (
        Id int IDENTITY(1,1) NOT NULL,
        Name nvarchar(100) NOT NULL,
        Description nvarchar(500) NULL,
        Title nvarchar(200) NOT NULL,
        Content nvarchar(max) NULL,
        Color nvarchar(7) NULL,
        Category nvarchar(50) NOT NULL,
        IsPublic bit NOT NULL DEFAULT 0,
        UsageCount int NOT NULL DEFAULT 0,
        UserId int NULL,
        CreatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted bit NOT NULL DEFAULT 0,
        DeletedAt datetime2(7) NULL,
        CONSTRAINT PK_NoteTemplates PRIMARY KEY (Id),
        CONSTRAINT FK_NoteTemplates_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users (Id) ON DELETE SET NULL
    );

    CREATE INDEX IX_NoteTemplates_UserId ON dbo.NoteTemplates (UserId);
    CREATE INDEX IX_NoteTemplates_Category ON dbo.NoteTemplates (Category);
    CREATE INDEX IX_NoteTemplates_IsPublic ON dbo.NoteTemplates (IsPublic);
    CREATE INDEX IX_NoteTemplates_UserId_Name ON dbo.NoteTemplates (UserId, Name);
    
    PRINT 'NoteTemplates table created successfully.';
END
ELSE
BEGIN
    PRINT 'NoteTemplates table already exists.';
END
GO

-- =============================================
-- Create NoteHistory Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NoteHistory')
BEGIN
    CREATE TABLE dbo.NoteHistory (
        Id int IDENTITY(1,1) NOT NULL,
        NoteId int NOT NULL,
        UserId int NOT NULL,
        Action nvarchar(50) NOT NULL,
        PreviousTitle nvarchar(200) NULL,
        PreviousContent nvarchar(max) NULL,
        NewTitle nvarchar(200) NULL,
        NewContent nvarchar(max) NULL,
        PreviousColor nvarchar(7) NULL,
        NewColor nvarchar(7) NULL,
        ChangeDetails nvarchar(1000) NULL,
        ActionDateTime datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        CreatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2(7) NOT NULL DEFAULT GETUTCDATE(),
        IsDeleted bit NOT NULL DEFAULT 0,
        DeletedAt datetime2(7) NULL,
        CONSTRAINT PK_NoteHistory PRIMARY KEY (Id),
        CONSTRAINT FK_NoteHistory_Notes_NoteId FOREIGN KEY (NoteId) REFERENCES dbo.Notes (Id) ON DELETE CASCADE,
        CONSTRAINT FK_NoteHistory_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
    );

    CREATE INDEX IX_NoteHistory_NoteId ON dbo.NoteHistory (NoteId);
    CREATE INDEX IX_NoteHistory_UserId ON dbo.NoteHistory (UserId);
    CREATE INDEX IX_NoteHistory_ActionDateTime ON dbo.NoteHistory (ActionDateTime);
    CREATE INDEX IX_NoteHistory_NoteId_ActionDateTime ON dbo.NoteHistory (NoteId, ActionDateTime);
    
    PRINT 'NoteHistory table created successfully.';
END
ELSE
BEGIN
    PRINT 'NoteHistory table already exists.';
END
GO

-- =============================================
-- Insert Sample Templates
-- =============================================
IF NOT EXISTS (SELECT * FROM dbo.NoteTemplates WHERE Name = 'Meeting Notes')
BEGIN
    INSERT INTO dbo.NoteTemplates (Name, Description, Title, Content, Color, Category, IsPublic, UserId)
    VALUES 
        ('Meeting Notes', 'Template for meeting notes', 'Meeting - [Date]', 
         'Attendees:\n- \n\nAgenda:\n1. \n2. \n3. \n\nNotes:\n\n\nAction Items:\n- [ ] \n- [ ] \n\nNext Meeting: ', 
         '#E1F5FE', 'Work', 1, NULL),
        
        ('Shopping List', 'Template for shopping lists', 'Shopping List', 
         '- [ ] \n- [ ] \n- [ ] \n- [ ] \n- [ ] ', 
         '#F3E5F5', 'Personal', 1, NULL),
        
        ('Travel Checklist', 'Template for travel planning', 'Travel Checklist - [Destination]', 
         'Before Travel:\n- [ ] Book flights\n- [ ] Book accommodation\n- [ ] Check passport/visa\n- [ ] Travel insurance\n\nPacking:\n- [ ] Clothes\n- [ ] Documents\n- [ ] Electronics\n- [ ] Medications\n\nDuring Travel:\n- [ ] Check-in\n- [ ] Confirm bookings', 
         '#FFF9C4', 'Travel', 1, NULL),
        
        ('Daily Journal', 'Template for daily journaling', 'Journal - [Date]', 
         'Today I am grateful for:\n1. \n2. \n3. \n\nToday''s highlights:\n- \n\nWhat I learned:\n\n\nTomorrow I will:\n- ', 
         '#CCFF90', 'Personal', 1, NULL),
        
        ('Project Planning', 'Template for project planning', 'Project: [Name]', 
         'Project Overview:\n\n\nObjectives:\n1. \n2. \n3. \n\nTimeline:\n- Start Date: \n- End Date: \n\nMilestones:\n- [ ] \n- [ ] \n\nResources Needed:\n- \n\nRisks:\n- ', 
         '#AECBFA', 'Work', 1, NULL);
    
    PRINT 'Sample templates inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Sample templates already exist.';
END
GO

PRINT 'Enhanced Google Keep features migration completed successfully!';
GO
