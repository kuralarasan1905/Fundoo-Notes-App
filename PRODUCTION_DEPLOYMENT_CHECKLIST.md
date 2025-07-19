# Production Deployment Checklist - Fundoo Notes API

## **Pre-Deployment Analysis Results**

### **Issues Fixed:**
1. **Hardcoded connection strings** in PowerShell scripts - FIXED
2. **Hardcoded IP addresses** in Program.cs - FIXED
3. **Missing production comments** in configuration files - FIXED
4. **Inconsistent connection strings** across environments - FIXED

### **Current Configuration Status:**

#### **Development Environment (appsettings.json):**
```json
// CORRECT - Uses Windows Authentication for local development
"DefaultConnection": "Server=KURALARASAN\\SQLEXPRESS;Database=FundooNotesDB;Integrated Security=true;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"
```

#### **Production Environment (appsettings.Production.json):**
```json
// ️ NEEDS UPDATE - Replace YOUR_PRODUCTION_SERVER with actual server
"DefaultConnection": "Server=YOUR_PRODUCTION_SERVER;Database=FundooNotesDB;User Id=appuser;Password=kural1905;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

## 🎯 **Production Deployment Steps**

### **Step 1: Database Server Setup**

#### **Option A: Use Same Server with SQL Authentication**
```json
"DefaultConnection": "Server=KURALARASAN\\SQLEXPRESS;Database=FundooNotesDB;User Id=appuser;Password=kural1905;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;"
```

#### **Option B: Use Remote Production Server**
```json
"DefaultConnection": "Server=192.168.1.100,1433;Database=FundooNotesDB;User Id=appuser;Password=kural1905;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

#### **Option C: Use Cloud Database (Azure SQL)**
```json
"DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=FundooNotesDB;Persist Security Info=False;User ID=appuser;Password=kural1905;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### **Step 2: Security Configuration**

#### **Update JWT Secret Key (CRITICAL):**
```json
"JwtSettings": {
  "SecretKey": "CHANGE-THIS-TO-A-UNIQUE-PRODUCTION-SECRET-KEY-AT-LEAST-32-CHARACTERS-LONG!",
  "Issuer": "FundooNotesAPI",
  "Audience": "FundooNotesClient",
  "ExpirationInMinutes": 60
}
```

#### **Update Email Settings:**
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "arasankural91@gmail.com",
  "SenderPassword": "your-production-app-password",
  "SenderName": "Fundoo Notes"
}
```

### **Step 3: Environment-Specific Settings**

#### **Update Allowed Hosts:**
```json
"AllowedHosts": "your-production-domain.com,www.your-production-domain.com"
```

#### **Update CORS Origins:**
```json
"AllowedOrigins": [
  "https://your-frontend-domain.com",
  "https://www.your-frontend-domain.com"
]
```

## 🔒 **Security Checklist**

### **Database Security:**
- [ ] Use SQL Authentication instead of Windows Authentication
- [ ] Enable encryption (`Encrypt=True`)
- [ ] Validate certificates (`TrustServerCertificate=False`)
- [ ] Use strong passwords
- [ ] Restrict database user permissions
- [ ] Enable SQL Server authentication mode

### **Application Security:**
- [ ] Change JWT SecretKey to unique production value
- [ ] Update email credentials for production
- [ ] Set specific allowed hosts (not "*")
- [ ] Configure proper CORS origins
- [ ] Enable HTTPS in production
- [ ] Set secure logging levels

### **Infrastructure Security:**
- [ ] Use firewall rules to restrict database access
- [ ] Enable SSL/TLS for database connections
- [ ] Set up proper backup strategy
- [ ] Monitor database connections
- [ ] Implement rate limiting

## 📝 **Configuration Files to Update**

### **1. appsettings.Production.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_ACTUAL_PRODUCTION_SERVER;Database=FundooNotesDB;User Id=appuser;Password=kural1905;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_UNIQUE_PRODUCTION_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG",
    "Issuer": "FundooNotesAPI",
    "Audience": "FundooNotesClient",
    "ExpirationInMinutes": 60
  },
  "AllowedHosts": "your-production-domain.com"
}
```

### **2. Database User Setup (if using SQL Authentication)**
```sql
-- Run this on your production SQL Server
USE master;
GO

-- Create login (if not exists)
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'appuser')
BEGIN
    CREATE LOGIN appuser WITH PASSWORD = 'kural1905';
END
GO

-- Switch to your database
USE FundooNotesDB;
GO

-- Create user and assign permissions
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'appuser')
BEGIN
    CREATE USER appuser FOR LOGIN appuser;
    ALTER ROLE db_owner ADD MEMBER appuser;
END
GO
```

## 🧪 **Testing Your Production Configuration**

### **Test Database Connection:**
```bash
# Test the production connection string
sqlcmd -S YOUR_PRODUCTION_SERVER -U appuser -P kural1905 -d FundooNotesDB -Q "SELECT COUNT(*) FROM Users"
```

### **Test Application Startup:**
```bash
# Run with production configuration
dotnet run --environment Production
```

### **Test API Endpoints:**
```bash
# Test health check
curl https://your-production-domain.com/api/health

# Test authentication
curl -X POST https://your-production-domain.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "kuralarasan1905@gmail.com", "password": "your-password"}'
```

## 🚨 **Critical Production Notes**

### **Your Connection String Analysis:**
The connection string you mentioned:
```
$connectionString = "Server=KURALARASAN\\SQLEXPRESS;Database=master;User Id=appuser;Password=kural1905;Connection Timeout=5;Encrypt=False;TrustServerCertificate=True;"
```

**Issues with this connection:**
- **Database=master** - Should be `Database=FundooNotesDB`
- **KURALARASAN\\SQLEXPRESS** - Won't work on production servers
- ️ **Encrypt=False** - Should be `True` for production
-  **SQL Authentication** - Correct for production

### **Corrected Production Connection:**
```json
"DefaultConnection": "Server=YOUR_PRODUCTION_SERVER;Database=FundooNotesDB;User Id=appuser;Password=kural1905;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

## **Final Deployment Command**

```bash
# Build and publish for production
dotnet publish -c Release -o ./publish

# Copy production configuration
cp appsettings.Production.json ./publish/

# Deploy to production server
# (Copy ./publish folder to production server)

# Start application on production server
cd ./publish
dotnet fundoo-notes.dll --environment Production
```

**Your backend is now properly configured for production deployment!**
