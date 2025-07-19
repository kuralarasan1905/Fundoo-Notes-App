# Testing Reminder Endpoints

## Problem Fixed
Your frontend was sending camelCase properties (`noteIdList`, `reminderDateTime`) but your ASP.NET Core backend expected PascalCase properties (`NoteIdList`, `ReminderDateTime`).

## Solution Applied
1. **Enhanced JSON Configuration**: Added `PropertyNameCaseInsensitive = true` to handle both cases
2. **Better Validation**: Added comprehensive validation with helpful error messages
3. **Improved Error Responses**: Clear error messages showing expected format

## Test the Fixed Endpoints

### 1. Add/Update Reminder for Notes
```
POST /api/Notes/addUpdateReminderNotes
Content-Type: application/json

{
  "noteIdList": ["1", "2"],
  "reminderDateTime": "2024-12-31T10:00:00Z"
}
```

**OR with PascalCase (both work now):**
```json
{
  "NoteIdList": ["1", "2"],
  "ReminderDateTime": "2024-12-31T10:00:00Z"
}
```

### 2. Remove Reminder from Notes
```
POST /api/Notes/removeReminderNotes
Content-Type: application/json

{
  "noteIdList": ["1", "2"]
}
```

**OR with PascalCase:**
```json
{
  "NoteIdList": ["1", "2"]
}
```

## Expected Responses

### Success Response:
```json
{
  "message": "Reminders updated for 2 notes",
  "updatedNotes": [
    {
      "id": 1,
      "title": "Test Note",
      "content": "Content",
      "reminderDateTime": "2024-12-31T10:00:00Z",
      // ... other properties
    }
  ]
}
```

### Validation Error Response:
```json
{
  "message": "Validation failed",
  "errors": [
    "Note IDs list cannot be empty",
    "Reminder date and time is required"
  ],
  "expectedFormat": {
    "noteIdList": ["1", "2", "3"],
    "reminderDateTime": "2024-12-31T10:00:00Z"
  }
}
```

## Common Issues and Solutions

### Issue 1: 400 Bad Request - Empty Note IDs
**Error**: "Note IDs list cannot be empty"
**Solution**: Ensure `noteIdList` contains valid note IDs

### Issue 2: 400 Bad Request - Invalid Date
**Error**: "Reminder date and time must be in the future"
**Solution**: Use a future date in ISO 8601 format

### Issue 3: 400 Bad Request - Invalid Note IDs
**Error**: "Invalid note IDs: abc, xyz"
**Solution**: Use numeric strings for note IDs

## Frontend Fix Summary

Your frontend should now work with either:
- **camelCase**: `noteIdList`, `reminderDateTime`
- **PascalCase**: `NoteIdList`, `ReminderDateTime`

The backend now accepts both formats thanks to the `PropertyNameCaseInsensitive = true` configuration.

## Test Steps

1. **Create a test note** first using `/api/Notes/addNotes`
2. **Get the note ID** from the response
3. **Test adding reminder** using the note ID
4. **Verify the reminder** by getting the note details
5. **Test removing reminder** using the same note ID

## Swagger Testing

You can also test these endpoints in Swagger UI at:
- `http://localhost:5000/swagger` (or your configured port)

The Swagger UI will show the expected request format and allow you to test directly.
