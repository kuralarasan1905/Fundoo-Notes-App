# Database Migration for Color Patterns

## Create Migration

Run this command in your project directory to create a migration for the color column changes:

```bash
dotnet ef migrations add UpdateColorColumnForPatterns
```

This will create a migration to:
1. Increase the `Color` column length from 7 to 50 characters in the `Notes` table
2. Increase the `Color` column length from 7 to 50 characters in the `NoteTemplates` table

## Apply Migration

After creating the migration, apply it to your database:

```bash
dotnet ef database update
```

## Manual SQL (if needed)

If you prefer to run the SQL manually, here's what the migration should contain:

```sql
-- Update Notes table
ALTER TABLE Notes ALTER COLUMN Color NVARCHAR(50);

-- Update NoteTemplates table  
ALTER TABLE NoteTemplates ALTER COLUMN Color NVARCHAR(50);
```

## Verify Changes

After running the migration, you can verify the changes by:

1. Checking your database schema
2. Testing the new color pattern functionality
3. Using the `/api/Notes/colors` endpoint to see available colors and patterns

## Test Data

You can insert some test notes with patterns:

```sql
-- Test notes with patterns (after migration)
INSERT INTO Notes (Title, Content, Color, UserId, CreatedAt, UpdatedAt) 
VALUES 
('Grid Pattern Note', 'This note has a grid pattern', 'grid', 1, GETUTCDATE(), GETUTCDATE()),
('Lines Pattern Note', 'This note has lines pattern', 'lines', 1, GETUTCDATE(), GETUTCDATE()),
('Dots Pattern Note', 'This note has dots pattern', 'dots', 1, GETUTCDATE(), GETUTCDATE()),
('Gradient Pattern Note', 'This note has gradient pattern', 'gradient', 1, GETUTCDATE(), GETUTCDATE());
```
