-- =============================================
-- Create LoginHistory Table
-- This script creates the missing LoginHistory table
-- Run this script to enable login history tracking
-- =============================================

USE [FundooNotesDB]
GO

-- Check if table exists and drop if it does (for clean recreation)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoginHistory]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[LoginHistory]
    PRINT 'Existing LoginHistory table dropped'
END
GO

-- Create LoginHistory table
CREATE TABLE [dbo].[LoginHistory](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [IpAddress] [nvarchar](45) NOT NULL,
    [UserAgent] [nvarchar](500) NULL,
    [Device] [nvarchar](50) NULL,
    [Location] [nvarchar](100) NULL,
    [IsSuccessful] [bit] NOT NULL,
    [FailureReason] [nvarchar](200) NULL,
    [LoginTime] [datetime2](7) NOT NULL,
    [LogoutTime] [datetime2](7) NULL,
    [CreatedAt] [datetime2](7) NOT NULL,
    [UpdatedAt] [datetime2](7) NOT NULL,
    [IsDeleted] [bit] NOT NULL DEFAULT 0,
    [DeletedAt] [datetime2](7) NULL,
    
    CONSTRAINT [PK_LoginHistory] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LoginHistory_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id])
)
GO

-- Create indexes for better performance
CREATE NONCLUSTERED INDEX [IX_LoginHistory_UserId] ON [dbo].[LoginHistory]([UserId])
GO

CREATE NONCLUSTERED INDEX [IX_LoginHistory_IpAddress] ON [dbo].[LoginHistory]([IpAddress])
GO

CREATE NONCLUSTERED INDEX [IX_LoginHistory_LoginTime] ON [dbo].[LoginHistory]([LoginTime])
GO

CREATE NONCLUSTERED INDEX [IX_LoginHistory_IsSuccessful] ON [dbo].[LoginHistory]([IsSuccessful])
GO

PRINT 'LoginHistory table created successfully with indexes'
PRINT 'You can now enable login history tracking in LoginHistoryService.cs'
GO
