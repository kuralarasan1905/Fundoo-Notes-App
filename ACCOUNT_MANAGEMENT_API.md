# Fundoo Notes - Complete Account Management API Documentation

## Overview
This document provides comprehensive documentation for the account management features implemented in Fundoo Notes API, following Google Keep's design patterns and professional standards.

## 🔐 Authentication Endpoints

### POST /api/auth/register
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
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-07-11T10:30:00Z",
  "user": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "fullName": "John Doe",
    "isEmailVerified": false,
    "isActive": true,
    "createdAt": "2025-07-10T10:30:00Z"
  }
}
```

### POST /api/auth/login
Login with email and password.

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "password": "SecurePassword123!"
}
```

**Response:** Same as registration response.

### POST /api/auth/forgot-password
Request password reset email.

**Request Body:**
```json
{
  "email": "john.doe@example.com"
}
```

**Response:**
```json
{
  "message": "If an account with that email exists, a password reset link has been sent.",
  "success": true
}
```

### POST /api/auth/reset-password
Reset password using token from email.

**Request Body:**
```json
{
  "token": "reset-token-from-email",
  "newPassword": "NewSecurePassword123!",
  "confirmNewPassword": "NewSecurePassword123!"
}
```

**Response:**
```json
{
  "message": "Password reset successfully"
}
```

### POST /api/auth/verify-email
Verify email address using token.

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "token": "verification-token-from-email"
}
```

**Response:**
```json
{
  "message": "Email verified successfully"
}
```

### POST /api/auth/resend-verification
Resend email verification.

**Request Body:**
```json
{
  "email": "john.doe@example.com"
}
```

**Response:**
```json
{
  "message": "If an account with that email exists and is not verified, a verification email has been sent.",
  "success": true
}
```

## 👤 Account Management Endpoints

### GET /api/account/profile
Get current user profile information.

**Headers:** `Authorization: Bearer {token}`

**Response:**
```json
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "isEmailVerified": true,
  "isActive": true,
  "lastLoginAt": "2025-07-10T09:15:00Z",
  "createdAt": "2025-07-10T08:00:00Z",
  "updatedAt": "2025-07-10T09:15:00Z",
  "totalNotes": 25,
  "totalLabels": 8
}
```

### PUT /api/account/profile
Update user profile information.

**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@example.com"
}
```

**Response:** Updated profile object (same structure as GET profile).

### POST /api/account/change-password
Change user password.

**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "currentPassword": "CurrentPassword123!",
  "newPassword": "NewSecurePassword123!",
  "confirmNewPassword": "NewSecurePassword123!"
}
```

**Response:**
```json
{
  "message": "Password changed successfully"
}
```

### GET /api/account/settings
Get account settings.

**Headers:** `Authorization: Bearer {token}`

**Response:**
```json
{
  "emailNotifications": true,
  "pushNotifications": true,
  "theme": "light",
  "language": "en",
  "timeZone": "UTC"
}
```

### PUT /api/account/settings
Update account settings.

**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "emailNotifications": false,
  "pushNotifications": true,
  "theme": "dark",
  "language": "en",
  "timeZone": "America/New_York"
}
```

**Response:** Updated settings object.

### POST /api/account/logout
Logout current session.

**Headers:** `Authorization: Bearer {token}`

**Response:**
```json
{
  "message": "Logged out successfully",
  "timestamp": "2025-07-10T10:30:00Z"
}
```

### POST /api/account/logout-all
Logout from all devices.

**Headers:** `Authorization: Bearer {token}`

**Response:**
```json
{
  "message": "Logged out from all devices successfully",
  "timestamp": "2025-07-10T10:30:00Z"
}
```

### POST /api/account/deactivate
Deactivate user account.

**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "password": "CurrentPassword123!",
  "reason": "No longer need the service"
}
```

**Response:**
```json
{
  "message": "Account deactivated successfully. You have been logged out.",
  "timestamp": "2025-07-10T10:30:00Z"
}
```

### GET /api/account/login-history
Get user login history.

**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `limit` (optional): Number of records to return (1-50, default: 10)

**Response:**
```json
[
  {
    "id": 1,
    "ipAddress": "192.168.1.100",
    "device": "Desktop",
    "location": "Unknown",
    "isSuccessful": true,
    "failureReason": null,
    "loginTime": "2025-07-10T09:15:00Z",
    "logoutTime": "2025-07-10T10:30:00Z",
    "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36..."
  }
]
```

## 🔒 Security Features

### Token Blacklisting
- Logout invalidates the current JWT token
- Logout-all invalidates all user tokens
- Account deactivation invalidates all user tokens

### Login History Tracking
- Records successful and failed login attempts
- Tracks IP address, device, and user agent
- Provides security audit trail

### Rate Limiting
- Failed login attempts are tracked by IP address
- Can be used to implement rate limiting (future enhancement)

### Password Security
- Passwords are hashed using BCrypt
- Password complexity requirements enforced
- Current password verification for sensitive operations

## 📧 Email Notifications

### Email Verification
- Sent automatically upon registration
- Can be resent if needed
- Required for full account functionality

### Password Reset
- Secure token-based password reset
- Tokens expire after 1 hour
- Email contains reset link

### Account Deactivation
- Confirmation email sent when account is deactivated
- Includes reason for deactivation

## 🚀 Production Considerations

### Security Enhancements
1. **HTTPS Only**: Ensure all endpoints use HTTPS in production
2. **Rate Limiting**: Implement rate limiting for authentication endpoints
3. **CORS**: Configure proper CORS policies
4. **Token Refresh**: Consider implementing refresh tokens for longer sessions

### Monitoring
1. **Login History**: Track and monitor suspicious login patterns
2. **Failed Attempts**: Alert on excessive failed login attempts
3. **Account Changes**: Log all profile and security changes

### Performance
1. **Caching**: Cache user profiles and settings
2. **Database Indexing**: Ensure proper indexes on login history
3. **Token Storage**: Consider Redis for token blacklisting in production

## 🧪 Testing

### Test User Account
- Email: `kuralarasan1905@gmail.com`
- Password: Available in your database
- Use this account for testing all endpoints

### Postman Collection
Import the API endpoints into Postman for easy testing:
1. Set base URL: `https://localhost:7139`
2. Add Authorization header with Bearer token
3. Test all endpoints with valid and invalid data

## 📝 Error Handling

All endpoints return consistent error responses:

```json
{
  "error": "Error message",
  "statusCode": 400,
  "timestamp": "2025-07-10T10:30:00Z"
}
```

Common HTTP status codes:
- `200`: Success
- `400`: Bad Request (validation errors)
- `401`: Unauthorized (invalid/missing token)
- `404`: Not Found
- `500`: Internal Server Error
