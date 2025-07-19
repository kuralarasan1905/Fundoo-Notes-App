-- =============================================
-- Add Test Reminders to Existing Notes
-- =============================================
-- This script adds reminder dates to existing notes for testing the reminders API

USE FundooNotesDB;
GO

-- Update existing notes with reminder dates for testing
UPDATE dbo.Notes 
SET ReminderDateTime = DATEADD(HOUR, 2, GETUTCDATE()),
    UpdatedAt = GETUTCDATE()
WHERE Title = 'Meeting Notes' AND UserId = 1;

UPDATE dbo.Notes 
SET ReminderDateTime = DATEADD(DAY, 1, GETUTCDATE()),
    UpdatedAt = GETUTCDATE()
WHERE Title = 'Shopping List' AND UserId = 1;

-- Insert additional test notes with reminders
INSERT INTO dbo.Notes (Title, Content, Color, ReminderDateTime, UserId, IsPinned, IsArchived, IsTrashed, IsDeleted, CreatedAt, UpdatedAt)
VALUES
    ('Doctor Appointment', 'Annual checkup with Dr. Smith at 2:00 PM', '#FFCDD2', DATEADD(DAY, 3, GETUTCDATE()), 1, 0, 0, 0, 0, GETUTCDATE(), GETUTCDATE()),
    ('Call Mom', 'Remember to call mom about weekend plans', '#C8E6C9', DATEADD(HOUR, 6, GETUTCDATE()), 1, 0, 0, 0, 0, GETUTCDATE(), GETUTCDATE()),
    ('Project Deadline', 'Submit final project report by end of week', '#FFE0B2', DATEADD(DAY, 5, GETUTCDATE()), 1, 0, 0, 0, 0, GETUTCDATE(), GETUTCDATE());

PRINT 'Test reminders added successfully!';

-- Verify the reminders were added
SELECT 
    Id,
    Title,
    ReminderDateTime,
    CASE 
        WHEN ReminderDateTime > GETUTCDATE() THEN 'Upcoming'
        ELSE 'Past Due'
    END as Status
FROM dbo.Notes 
WHERE ReminderDateTime IS NOT NULL AND UserId = 1
ORDER BY ReminderDateTime;

GO
