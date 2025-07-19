# 🔐 JWT Authentication Complete Guide

## **How JWT Authentication Works Between Frontend and Backend**

### **1. The Complete Flow:**

```
┌─────────────┐    1. Login Request     ┌─────────────┐
│   Frontend  │ ────────────────────► │   Backend   │
│             │                        │             │
│             │ ◄──────────────────── │             │
└─────────────┘    2. JWT Token        └─────────────┘
       │                                      ▲
       │ 3. Store Token                       │
       │ localStorage.setItem('token', jwt)   │
       ▼                                      │
┌─────────────┐    4. API Calls with Token   │
│ localStorage│ ────────────────────────────┘
│ token: jwt  │    Authorization: Bearer jwt
└─────────────┘
```

### **2. Frontend Implementation (Angular):**

#### **Step 1: Login and Store Token**
```typescript
// In your login service
login(credentials: any): Observable<any> {
  return this.http.post('/api/auth/login', credentials).pipe(
    tap((response: any) => {
      // Store the JWT token
      localStorage.setItem('token', response.token);
      console.log(' JWT token stored:', response.token);
    })
  );
}
```

#### **Step 2: Send Token with Every API Call**
```typescript
// In your HTTP service (what we implemented)
getHeader() {
  const token = localStorage.getItem('token');
  
  return new HttpHeaders({
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`  // This is crucial!
  });
}
```

#### **Step 3: Use Headers in API Calls**
```typescript
// Every API call should use these headers
this.http.get('/api/notes', { headers: this.getHeader() })
this.http.post('/api/notes', data, { headers: this.getHeader() })
```

### **3. Backend Implementation (ASP.NET Core):**

#### **Step 1: Configure JWT in Program.cs**
```csharp
// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "your-issuer",
            ValidAudience = "your-audience",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("your-secret-key-must-be-at-least-32-characters"))
        };
    });

// Add Authorization
builder.Services.AddAuthorization();

// Configure the HTTP request pipeline
app.UseAuthentication();  // Must come before UseAuthorization
app.UseAuthorization();
```

#### **Step 2: Protect Controllers**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // This requires JWT token for ALL endpoints
[Produces("application/json")]
public class NotesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetNotes()
    {
        // This endpoint requires valid JWT token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok($"Hello user {userId}");
    }
}
```

### **4. Common Issues and Solutions:**

#### **Issue 1: Token Not Being Sent**
**Problem:** Frontend doesn't send Authorization header
**Solution:** 
```typescript
// Make sure you're using getHeader() in ALL API calls
this.http.get('/api/notes', { headers: this.getHeader() })
```

#### **Issue 2: Wrong Token Format**
**Problem:** Authorization header format is incorrect
**Solution:**
```typescript
// Correct format:
'Authorization': `Bearer ${token}`

// Wrong formats:
'Authorization': token           // Missing "Bearer "
'Authorization': `${token}`      // Missing "Bearer "
'Authorization': `bearer ${token}` // Wrong case
```

#### **Issue 3: Token Not Stored After Login**
**Problem:** Login doesn't store token properly
**Solution:**
```typescript
// In login response handler:
localStorage.setItem('token', response.token);  // Make sure this happens
```

#### **Issue 4: Token Expired**
**Problem:** JWT token has expired
**Solution:**
```typescript
// Check token expiration before using
const token = localStorage.getItem('token');
if (token) {
  const payload = JSON.parse(atob(token.split('.')[1]));
  const expiry = new Date(payload.exp * 1000);
  if (expiry < new Date()) {
    // Token expired - redirect to login
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
}
```

### **5. Testing JWT Authentication:**

#### **Browser Console Commands:**
```javascript
// Check if token exists
localStorage.getItem('token')

// Check token details
checkToken()

// Test authentication
testJwtAuth()

// Enable/disable auth for testing
enableAuth()   // Enable JWT authentication
disableAuth()  // Disable JWT authentication
```

### **6. Network Request Example:**

#### **What Frontend Sends:**
```http
POST /api/notes/addNotes HTTP/1.1
Host: localhost:7256
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "title": "My Note",
  "content": "Note content"
}
```

#### **What Backend Receives:**
```csharp
// In your controller, you can access user info:
[HttpPost("addNotes")]
public IActionResult AddNote([FromBody] CreateNoteDto note)
{
    // Get user ID from JWT token
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
    
    // Use userId to associate note with user
    note.UserId = userId;
    
    return Ok("Note created for user: " + userEmail);
}
```

### **7. Debugging Steps:**

1. **Check if user is logged in:**
   ```javascript
   console.log('Token:', localStorage.getItem('token'));
   ```

2. **Verify token format:**
   ```javascript
   checkToken()
   ```

3. **Test authentication:**
   ```javascript
   testJwtAuth()
   ```

4. **Check network requests:**
   - Open browser DevTools → Network tab
   - Look for Authorization header in request headers
   - Should see: `Authorization: Bearer [your-jwt-token]`

5. **Check backend logs:**
   - Look for authentication errors
   - Verify JWT configuration is correct

### **8. Complete Working Example:**

#### **Frontend Login:**
```typescript
login(email: string, password: string) {
  return this.http.post('/api/auth/login', { email, password })
    .pipe(
      tap((response: any) => {
        localStorage.setItem('token', response.token);
        console.log(' Logged in, token stored');
      })
    );
}
```

#### **Frontend API Call:**
```typescript
getNotes() {
  const headers = this.getHeader(); // Includes Authorization: Bearer [token]
  return this.http.get('/api/notes', { headers });
}
```

#### **Backend Controller:**
```csharp
[Authorize]
[HttpGet]
public IActionResult GetNotes()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var notes = _noteService.GetNotesByUserId(userId);
    return Ok(notes);
}
```

This is the complete flow for JWT authentication! The key is making sure the token is properly stored after login and sent with every API request.
