# Fundoo Notes API Documentation

## Base URL
- **Development**: `https://localhost:7139` or `http://localhost:5139`
- **Production**: Update with your production URL

## Authentication
All endpoints except authentication endpoints require a JWT Bearer token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

## Response Format
All API responses follow a consistent format:

### Success Response
```json
{
  "data": { ... },
  "message": "Success message",
  "timestamp": "2025-01-XX..."
}
```

### Error Response
```json
{
  "error": "Error message",
  "details": "Detailed error information",
  "timestamp": "2025-01-XX..."
}
```

## Endpoints

### Health Check

#### GET /api/health
Check if the API is running.

**Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2025-01-XX...",
  "version": "1.0.0.0",
  "environment": "Development",
  "machineName": "...",
  "message": "Fundoo Notes API is running successfully"
}
```

### Authentication

#### POST /api/auth/register
Register a new user account.

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!"
}
```

**Response:**
```json
{
  "message": "Registration successful. Please check your email to verify your account.",
  "userId": 1
}
```

#### POST /api/auth/login
Login with email and password.

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "isEmailVerified": true
  },
  "expiresAt": "2025-01-XX..."
}
```

#### POST /api/auth/verify-email
Verify email address with token.

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "token": "verification-token-here"
}
```

#### POST /api/auth/forgot-password
Request password reset.

**Request Body:**
```json
{
  "email": "john.doe@example.com"
}
```

#### POST /api/auth/reset-password
Reset password with token.

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "token": "reset-token-here",
  "newPassword": "NewSecurePassword123!",
  "confirmPassword": "NewSecurePassword123!"
}
```

### Notes Management

#### GET /api/notes
Get current user's notes.

**Query Parameters:**
- `includeArchived` (boolean, optional): Include archived notes (default: false)
- `includeTrashed` (boolean, optional): Include trashed notes (default: false)

**Response:**
```json
[
  {
    "id": 1,
    "title": "My First Note",
    "Content": "This is the preview of my first note...",
    "color": "#FFF9C4",
    "isPinned": false,
    "isArchived": false,
    "isTrashed": false,
    "reminderDateTime": null,
    "hasReminder": false,
    "updatedAt": "2025-01-XX...",
    "labels": [
      {
        "id": 1,
        "name": "Personal",
        "color": "#FF5722"
      }
    ],
    "collaboratorCount": 0
  }
]
```

#### GET /api/notes/search
Search notes by title and content.

**Query Parameters:**
- `searchTerm` (string, required): Search term (minimum 2 characters)
- `includeArchived` (boolean, optional): Include archived notes (default: false)
- `includeTrashed` (boolean, optional): Include trashed notes (default: false)

**Response:** Same as GET /api/notes

#### GET /api/notes/{id}
Get a specific note by ID.

**Response:**
```json
{
  "id": 1,
  "title": "My First Note",
  "content": "This is the full content of my first note.",
  "Content": "This is the preview of my first note...",
  "color": "#FFF9C4",
  "isPinned": false,
  "isArchived": false,
  "isTrashed": false,
  "reminderDateTime": null,
  "hasReminder": false,
  "userId": 1,
  "userName": "John Doe",
  "createdAt": "2025-01-XX...",
  "updatedAt": "2025-01-XX...",
  "labels": [
    {
      "id": 1,
      "name": "Personal",
      "color": "#FF5722",
      "userId": 1,
      "createdAt": "2025-01-XX...",
      "updatedAt": "2025-01-XX...",
      "notesCount": 5
    }
  ],
  "collaborators": []
}
```

#### POST /api/notes
Create a new note.

**Request Body:**
```json
{
  "title": "My New Note",
  "content": "This is the content of my new note.",
  "color": "#FFF9C4",
  "reminderDateTime": "2025-02-01T10:00:00Z",
  "labelIds": [1, 2]
}
```

**Response:** Same as GET /api/notes/{id}

#### PUT /api/notes/{id}
Update an existing note.

**Request Body:**
```json
{
  "title": "Updated Note Title",
  "content": "Updated note content.",
  "color": "#E1F5FE",
  "reminderDateTime": null,
  "labelIds": [1]
}
```

**Response:** Same as GET /api/notes/{id}

#### PATCH /api/notes/{id}/pin
Toggle note pin status.

**Response:** Same as GET /api/notes/{id}

#### PATCH /api/notes/{id}/archive
Toggle note archive status.

**Response:** Same as GET /api/notes/{id}

#### DELETE /api/notes/{id}
Delete a note (soft delete).

**Response:** 204 No Content

### Labels Management

#### GET /api/labels
Get current user's labels.

**Response:**
```json
[
  {
    "id": 1,
    "name": "Personal",
    "color": "#FF5722",
    "userId": 1,
    "createdAt": "2025-01-XX...",
    "updatedAt": "2025-01-XX...",
    "notesCount": 5
  }
]
```

#### POST /api/labels
Create a new label.

**Request Body:**
```json
{
  "name": "Work",
  "color": "#2196F3"
}
```

**Response:**
```json
{
  "id": 2,
  "name": "Work",
  "color": "#2196F3",
  "userId": 1,
  "createdAt": "2025-01-XX...",
  "updatedAt": "2025-01-XX...",
  "notesCount": 0
}
```

#### PUT /api/labels/{id}
Update an existing label.

**Request Body:**
```json
{
  "name": "Updated Work",
  "color": "#1976D2"
}
```

**Response:** Same as POST /api/labels

#### DELETE /api/labels/{id}
Delete a label.

**Response:** 204 No Content

### Collaborators Management

#### GET /api/collaborators/note/{noteId}
Get collaborators for a specific note.

**Response:**
```json
[
  {
    "id": 1,
    "noteId": 1,
    "userId": 2,
    "userName": "Jane Doe",
    "userEmail": "jane@example.com",
    "permission": "Write",
    "isAccepted": true,
    "acceptedAt": "2025-01-XX...",
    "createdAt": "2025-01-XX..."
  }
]
```

#### POST /api/collaborators
Add a collaborator to a note.

**Request Body:**
```json
{
  "noteId": 1,
  "userEmail": "collaborator@example.com",
  "permission": "Read"
}
```

#### DELETE /api/collaborators/{id}
Remove a collaborator from a note.

**Response:** 204 No Content

#### PATCH /api/collaborators/{id}/permission
Update collaborator permission.

**Request Body:**
```json
{
  "permission": "Write"
}
```

### Templates Management

#### GET /api/templates
Get user's templates.

**Query Parameters:**
- `category` (optional): Filter by category

**Response:**
```json
[
  {
    "id": 1,
    "name": "Meeting Notes",
    "description": "Template for meeting notes",
    "title": "Meeting - [Date]",
    "content": "Attendees:\n- \n\nAgenda:\n1. \n2. \n3. \n\nNotes:\n\n\nAction Items:\n- [ ] \n- [ ] \n\nNext Meeting: ",
    "color": "#E1F5FE",
    "category": "Work",
    "isPublic": true,
    "userId": null,
    "createdAt": "2025-01-XX..."
  }
]
```

#### GET /api/templates/public
Get public templates available to all users.

#### POST /api/templates
Create a new template.

**Request Body:**
```json
{
  "name": "Custom Template",
  "description": "My custom template",
  "title": "Template Title",
  "content": "Template content...",
  "color": "#FFF9C4",
  "category": "Personal",
  "isPublic": false
}
```

### Attachments Management

#### GET /api/attachments/note/{noteId}
Get attachments for a specific note.

**Response:**
```json
[
  {
    "id": 1,
    "noteId": 1,
    "fileName": "document.pdf",
    "fileType": "application/pdf",
    "fileUrl": "/uploads/documents/document.pdf",
    "fileSize": 1024000,
    "createdAt": "2025-01-XX..."
  }
]
```

#### POST /api/attachments/note/{noteId}
Upload an attachment to a note.

**Request:** Multipart form data with file
**File size limit:** 10MB
**Supported types:** Images (JPEG, PNG, GIF, WebP), PDF, Text files

#### DELETE /api/attachments/{id}
Delete an attachment.

**Response:** 204 No Content

#### GET /api/attachments/{id}/download
Download an attachment file.

**Response:** File download

### Reminders Management

#### GET /api/reminders
Get user's upcoming reminders.

**Query Parameters:**
- `includeCompleted` (optional): Include completed reminders

**Response:**
```json
[
  {
    "id": 1,
    "noteId": 1,
    "reminderDateTime": "2025-02-01T10:00:00Z",
    "reminderType": "Once",
    "isCompleted": false,
    "completedAt": null,
    "createdAt": "2025-01-XX..."
  }
]
```

#### POST /api/reminders
Create a new reminder.

**Request Body:**
```json
{
  "noteId": 1,
  "reminderDateTime": "2025-02-01T10:00:00Z",
  "reminderType": "Daily"
}
```

#### PATCH /api/reminders/{id}/complete
Mark a reminder as completed.

**Response:** Updated reminder object

### Enhanced Notes Features

#### POST /api/notes/from-template/{templateId}
Create a note from a template.

**Response:** Created note object

#### GET /api/notes/{id}/history
Get note change history.

**Response:**
```json
[
  {
    "id": 1,
    "noteId": 1,
    "userId": 1,
    "userName": "John Doe",
    "action": "Updated",
    "previousTitle": "Old Title",
    "newTitle": "New Title",
    "changeDetails": "Title and content updated",
    "actionDateTime": "2025-01-XX..."
  }
]
```

#### PATCH /api/notes/{id}/color
Update note color.

**Request Body:**
```json
"#FF5722"
```

#### DELETE /api/notes/bulk
Bulk delete multiple notes.

**Request Body:**
```json
{
  "noteIds": [1, 2, 3, 4, 5]
}
```

**Response:** 204 No Content

## Error Codes

| Status Code | Description |
|-------------|-------------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 204 | No Content - Request successful, no content to return |
| 400 | Bad Request - Invalid request data |
| 401 | Unauthorized - Authentication required or invalid |
| 403 | Forbidden - Access denied |
| 404 | Not Found - Resource not found |
| 409 | Conflict - Resource already exists |
| 422 | Unprocessable Entity - Validation errors |
| 500 | Internal Server Error - Server error |

## Rate Limiting
- **Authentication endpoints**: 5 requests per minute per IP
- **Other endpoints**: 100 requests per minute per user

## Data Validation

### Password Requirements
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character

### Note Constraints
- Title: 1-200 characters (required)
- Content: 0-5000 characters
- Color: Valid hex color format (#RRGGBB or #RGB)

### Label Constraints
- Name: 1-50 characters (required, unique per user)
- Color: Valid hex color format (#RRGGBB or #RGB)

## Swagger Documentation
Interactive API documentation is available at:
- **Development**: `https://localhost:7139/swagger`
- **Production**: `{your-domain}/swagger`

## SDK and Client Libraries
Coming soon:
- JavaScript/TypeScript SDK
- C# SDK
- Python SDK

## Support
For API support and questions:
- Check the Swagger documentation
- Review error messages and status codes
- Ensure proper authentication headers
- Validate request data format
